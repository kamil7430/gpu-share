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
		return nil, err
	}

	client := proto.NewAgentServiceClient(conn)

	return client.Connect(ctx)
}

func SendHelloMessage(stream Stream, agentId string) error {
	return stream.Send(&proto.AgentMessage{
		AgentId: agentId,
	})
}

func SendHeartbeats(ctx context.Context, stream Stream, agentId string) error {
	ticker := time.NewTicker(2 * time.Second)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return ctx.Err()

		case <-ticker.C:
			err := stream.Send(&proto.AgentMessage{
				AgentId: agentId,
				Payload: &proto.AgentMessage_Heartbeat{
					Heartbeat: &proto.Heartbeat{
						GpuUtil: rand.Float32(),
					},
				},
			})
			if err != nil {
				log.Printf("couldn't send heartbeat (%v)\n", err)
			}
		}
	}
}

func ReceiveLoop(ctx context.Context, stream Stream, agentId string) error {
	for {
		select {
		case <-ctx.Done():
			return ctx.Err()
		default:
			msg, err := stream.Recv()
			if err != nil {
				return err
			}

			switch payload := msg.Payload.(type) {
			case *proto.CoordinatorMessage_Task:
				go ExecuteTask(ctx, stream, agentId, payload.Task)
			}
		}
	}
}

func ExecuteTask(ctx context.Context, stream proto.AgentService_ConnectClient, agentID string, task *proto.Task) {
	log.Println("Executing task:", task.TaskId)

	steps := 10
	for i := 1; i <= steps; i++ {
		dur := time.Duration(float32(task.MemoryMb)*0.01) * time.Second
		time.Sleep(dur / time.Duration(steps))

		msg := proto.AgentMessage{
			AgentId: agentID,
			Payload: &proto.AgentMessage_TaskUpdate{
				TaskUpdate: &proto.TaskUpdate{
					TaskId:   task.TaskId,
					Progress: float32(i) / float32(steps),
					Status:   "running",
				},
			},
		}
		if err := stream.Send(&msg); err != nil {
			log.Printf("couldn't send status message (%v)\n", err)
		}
	}

	msg := proto.AgentMessage{
		AgentId: agentID,
		Payload: &proto.AgentMessage_TaskUpdate{
			TaskUpdate: &proto.TaskUpdate{
				TaskId:   task.TaskId,
				Progress: 1.0,
				Status:   "done",
			},
		},
	}
	if err := stream.Send(&msg); err != nil {
		// TODO: we'll need some retry and result caching logic later
		log.Printf("couldn't send status message (%v)\n", err)
	}
}
