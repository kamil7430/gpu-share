package agent

import (
	"context"
	"log"
	"math/rand/v2"
	"time"

	"github.com/kamil7430/gpu-share/gpu/proto"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type Stream = grpc.BidiStreamingClient[proto.AgentMessage, proto.CoordinatorMessage]

func StartGrpcClient(ctx context.Context, url string) (Stream, error) {
	var opts []grpc.DialOption
	opts = append(opts, grpc.WithTransportCredentials(insecure.NewCredentials()))
	conn, err := grpc.NewClient(url, opts...)
	if err != nil {
		log.Fatal(err)
	}

	client := proto.NewAgentServiceClient(conn)

	return client.Connect(ctx)
}

func SendHelloMessage(stream Stream, agentId string) {
	err := stream.Send(&proto.AgentMessage{
		AgentId: agentId,
	})
	if err != nil {
		log.Fatal(err)
	}
}

func SendHeartbeats(stream Stream, agentId string) {
	for {
		time.Sleep(2 * time.Second)

		stream.Send(&proto.AgentMessage{
			AgentId: agentId,
			Payload: &proto.AgentMessage_Heartbeat{
				Heartbeat: &proto.Heartbeat{
					GpuUtil: rand.Float32(),
				},
			},
		})
	}
}

func ReceiveLoop(stream Stream, agentId string) {
	for {
		msg, err := stream.Recv()
		if err != nil {
			log.Fatal(err)
		}

		switch payload := msg.Payload.(type) {
		case *proto.CoordinatorMessage_Task:
			go ExecuteTask(stream, agentId, payload.Task)
		}
	}
}

func ExecuteTask(stream proto.AgentService_ConnectClient, agentID string, task *proto.Task) {
	log.Println("Executing task:", task.TaskId)

	steps := 10
	for i := 1; i <= steps; i++ {
		dur := time.Duration(float32(task.MemoryMb)*0.01) * time.Second
		time.Sleep(dur / time.Duration(steps))

		stream.Send(&proto.AgentMessage{
			AgentId: agentID,
			Payload: &proto.AgentMessage_TaskUpdate{
				TaskUpdate: &proto.TaskUpdate{
					TaskId:   task.TaskId,
					Progress: float32(i) / float32(steps),
					Status:   "running",
				},
			},
		})
	}

	stream.Send(&proto.AgentMessage{
		AgentId: agentID,
		Payload: &proto.AgentMessage_TaskUpdate{
			TaskUpdate: &proto.TaskUpdate{
				TaskId:   task.TaskId,
				Progress: 1.0,
				Status:   "done",
			},
		},
	})
}
