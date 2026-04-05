package service

import (
	"fmt"
	"log"

	"github.com/kamil7430/gpu-share/gpu/coordinator/repository"
	"github.com/kamil7430/gpu-share/gpu/proto"
)

type AgentService struct {
	proto.UnimplementedAgentServiceServer

	ar         *repository.AgentRepository
	lastTaskId int
}

func NewAgentService(ar *repository.AgentRepository) *AgentService {
	return &AgentService{ar: ar, lastTaskId: 0}
}

func (as *AgentService) Connect(stream proto.AgentService_ConnectServer) error {
	firstMsg, err := stream.Recv()
	if err != nil {
		return err
	}

	agentID := firstMsg.AgentId
	agent := &repository.Agent{
		Id:     agentID,
		Stream: stream,
		SendCh: make(chan *proto.CoordinatorMessage, 10),
	}

	as.ar.Register(agent)
	defer as.ar.Unregister(agentID)

	go func() {
		for msg := range agent.SendCh {
			_ = stream.Send(msg)
		}
	}()

	log.Printf("Agent '%v' connected\n", agentID)

	for {
		msg, err := stream.Recv()
		if err != nil {
			log.Println("Agent disconnected:", agentID)
			return err
		}

		as.handleMessage(agentID, msg)
	}
}

func (s *AgentService) handleMessage(agentID string, msg *proto.AgentMessage) {
	switch payload := msg.Payload.(type) {

	case *proto.AgentMessage_Heartbeat:
		log.Printf("Heartbeat from %s: gpu=%.2f\n",
			agentID, payload.Heartbeat.GpuUtil)

	case *proto.AgentMessage_TaskUpdate:
		log.Printf("Task update from %s: %s progress=%.2f status=%s\n",
			agentID,
			payload.TaskUpdate.TaskId,
			payload.TaskUpdate.Progress,
			payload.TaskUpdate.Status,
		)
	}
}

func (as *AgentService) SendTask(agentID string, memoryMb int) (taskId string, err error) {
	taskId = fmt.Sprintf("task-%v", as.lastTaskId)
	msg := proto.CoordinatorMessage{
		Payload: &proto.CoordinatorMessage_Task{
			Task: &proto.Task{
				Type:     "mock",
				MemoryMb: int32(memoryMb),
				TaskId:   taskId,
			},
		},
	}

	err = as.ar.SendTo(agentID, &msg)
	if err != nil {
		return
	}
	as.lastTaskId += 1

	return taskId, nil
}
