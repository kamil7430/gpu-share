package main

import (
	"context"
	"log"
	"math/rand"
	"time"

	pb "github.com/kamil7430/gpu-share/gpu/proto"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {
	var opts []grpc.DialOption
	opts = append(opts, grpc.WithTransportCredentials(insecure.NewCredentials()))
	conn, err := grpc.NewClient("localhost:2139", opts...)
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()

	client := pb.NewAgentServiceClient(conn)

	stream, err := client.Connect(context.Background())
	if err != nil {
		log.Fatal(err)
	}

	// TODO: who should assign this id???
	agentID := "agent-1"

	stream.Send(&pb.AgentMessage{
		AgentId: agentID,
	})

	go func() {
		for {
			time.Sleep(2 * time.Second)

			stream.Send(&pb.AgentMessage{
				AgentId: agentID,
				Payload: &pb.AgentMessage_Heartbeat{
					Heartbeat: &pb.Heartbeat{
						GpuUtil: rand.Float32(),
					},
				},
			})
		}
	}()

	for {
		msg, err := stream.Recv()
		if err != nil {
			log.Fatal(err)
		}

		switch payload := msg.Payload.(type) {
		case *pb.CoordinatorMessage_Task:
			go executeTask(stream, agentID, payload.Task)
		}
	}
}

func executeTask(stream pb.AgentService_ConnectClient, agentID string, task *pb.Task) {
	log.Println("Executing task:", task.TaskId)

	steps := 10
	for i := 1; i <= steps; i++ {
		dur := time.Duration(float32(task.MemoryMb)*0.01) * time.Second
		time.Sleep(dur / time.Duration(steps))

		stream.Send(&pb.AgentMessage{
			AgentId: agentID,
			Payload: &pb.AgentMessage_TaskUpdate{
				TaskUpdate: &pb.TaskUpdate{
					TaskId:   task.TaskId,
					Progress: float32(i) / float32(steps),
					Status:   "running",
				},
			},
		})
	}

	stream.Send(&pb.AgentMessage{
		AgentId: agentID,
		Payload: &pb.AgentMessage_TaskUpdate{
			TaskUpdate: &pb.TaskUpdate{
				TaskId:   task.TaskId,
				Progress: 1.0,
				Status:   "done",
			},
		},
	})
}
