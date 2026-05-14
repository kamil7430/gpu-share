package service

import (
	"context"
	"fmt"
	"log"

	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/repository"
	"github.com/kamil7430/gpu-share/gpu/proto"
)

type AgentService struct {
	proto.UnimplementedAgentServiceServer

	ar         *repository.AgentRepository
	dr         *repository.DeviceRepository
	lastTaskId int
}

func NewAgentService(ar *repository.AgentRepository) *AgentService {
	return &AgentService{ar: ar, lastTaskId: 0}
}

func (as *AgentService) verifyID(agentID string, token string) error {
	devices, err := as.dr.GetDevices(token)
	if err != nil {
		return err
	}
	found := false
	for _, d := range devices {
		if d.DeviceID == agentID {
			found = true
			break
		}
	}
	if !found {
		return fmt.Errorf("device %v not found in registered devices", agentID)
	}
	return nil
}

func (as *AgentService) Connect(stream proto.AgentService_ConnectServer) error {
	firstMsg, err := stream.Recv()
	if err != nil {
		return err
	}

	agentID := firstMsg.AgentId
	token := firstMsg.Token
	if err := as.verifyID(agentID, token); err != nil {
		return err
	}
	agent := &repository.Agent{
		Id:     agentID,
		Stream: stream,
		SendCh: make(chan *proto.CoordinatorMessage, 10),
	}

	if as.ar.IsConnected(agentID) {
		return fmt.Errorf("agent %v already connected", agentID)
	}
	as.ar.Register(agent)
	defer as.ar.Unregister(agentID)

	go func() {
		for msg := range agent.SendCh {
			if err := stream.Send(msg); err != nil {
				log.Printf("couldn't send message (%v)\n", err)
			}
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

func (as *AgentService) handleMessage(agentID string, msg *proto.AgentMessage) {
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

func (as *AgentService) ScheduleTask(ctx context.Context, r *api.ScheduleTaskReq) (api.ScheduleTaskRes, error) {
	jobId, err := as.SendTask(string(r.DeviceId), r.Resources.VRamMb)
	if err != nil {
		// TODO: probably should return sf else
		return nil, err
	}

	return &api.ScheduleTaskCreated{
		JobId: jobId,
	}, nil
}

func (as *AgentService) GetAgentStatus(ctx context.Context, params api.GetAgentStatusParams) (api.GetAgentStatusRes, error) {
	agentId := params.AgentId
	connected := as.ar.IsConnected(agentId)
	if connected {
		return &api.GetAgentStatusOK{}, nil
	} else {
		return &api.GetAgentStatusNotFound{}, nil
	}
}
