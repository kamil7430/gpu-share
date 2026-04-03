package main

import (
	"log"
	"net"
	"sync"

	pb "github.com/kamil7430/gpu-share/gpu/proto"
	"google.golang.org/grpc"
)

type Agent struct {
	id     string
	stream pb.AgentService_ConnectServer
	sendCh chan *pb.CoordinatorMessage
}

type Server struct {
	pb.UnimplementedAgentServiceServer

	mu     sync.Mutex
	agents map[string]*Agent
}

func NewServer() *Server {
	return &Server{
		agents: make(map[string]*Agent),
	}
}

func (s *Server) Connect(stream pb.AgentService_ConnectServer) error {
	firstMsg, err := stream.Recv()
	if err != nil {
		return err
	}

	agentID := firstMsg.AgentId
	agent := &Agent{
		id:     agentID,
		stream: stream,
		sendCh: make(chan *pb.CoordinatorMessage, 10),
	}

	s.register(agent)
	defer s.unregister(agentID)

	go func() {
		for msg := range agent.sendCh {
			_ = stream.Send(msg)
		}
	}()

	log.Println("Agent connected: ", agentID)

	agent.sendCh <- &pb.CoordinatorMessage{
		Payload: &pb.CoordinatorMessage_Task{
			Task: &pb.Task{
				TaskId:   "task-1",
				Type:     "mock",
				MemoryMb: 1000,
			},
		},
	}

	for {
		msg, err := stream.Recv()
		if err != nil {
			log.Println("Agent disconnected:", agentID)
			return err
		}

		s.handleMessage(agentID, msg)
	}
}

func (s *Server) handleMessage(agentID string, msg *pb.AgentMessage) {
	switch payload := msg.Payload.(type) {

	case *pb.AgentMessage_Heartbeat:
		log.Printf("Heartbeat from %s: gpu=%.2f\n",
			agentID, payload.Heartbeat.GpuUtil)

	case *pb.AgentMessage_TaskUpdate:
		log.Printf("Task update from %s: %s progress=%.2f status=%s\n",
			agentID,
			payload.TaskUpdate.TaskId,
			payload.TaskUpdate.Progress,
			payload.TaskUpdate.Status,
		)
	}
}

func (s *Server) register(a *Agent) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.agents[a.id] = a
}

func (s *Server) unregister(id string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	delete(s.agents, id)
}

func main() {
	port := ":12345"
	lis, err := net.Listen("tcp", port)
	if err != nil {
		log.Fatal(err)
	}

	grpcServer := grpc.NewServer()
	pb.RegisterAgentServiceServer(grpcServer, NewServer())

	log.Printf("Coordinator listening on %v\n", port)
	if err := grpcServer.Serve(lis); err != nil {
		log.Fatal(err)
	}
}
