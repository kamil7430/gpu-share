package repository

import (
	"fmt"
	"sync"

	"github.com/kamil7430/gpu-share/gpu/proto"
)

type Agent struct {
	Id     string
	Stream proto.AgentService_ConnectServer
	SendCh chan *proto.CoordinatorMessage
}

type AgentRepository struct {
	proto.UnimplementedAgentServiceServer

	mu     sync.Mutex
	agents map[string]*Agent
}

func NewAgentRepository() *AgentRepository {
	return &AgentRepository{
		agents: make(map[string]*Agent),
	}
}

func (ar *AgentRepository) Register(a *Agent) {
	ar.mu.Lock()
	defer ar.mu.Unlock()
	ar.agents[a.Id] = a
}

func (ar *AgentRepository) Unregister(id string) {
	ar.mu.Lock()
	defer ar.mu.Unlock()
	delete(ar.agents, id)
}

func (ar *AgentRepository) SendTo(agentId string, message *proto.CoordinatorMessage) error {
	agent, ok := ar.agents[agentId]
	if !ok {
		return fmt.Errorf("No agent found with id %v", agentId)
	}

	agent.SendCh <- message

	return nil
}
