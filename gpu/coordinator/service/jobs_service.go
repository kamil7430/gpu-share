package service

import (
	"context"

	"github.com/kamil7430/gpu-share/gpu/coordinator/api"
)

type JobsService struct{}

func NewJobsService() JobsService {
	return JobsService{}
}

func (s *JobsService) AddTask(ctx context.Context, r *api.AddTaskReq) (api.AddTaskRes, error) {
	return &api.AddTaskCreated{
		JobId: "job-123",
	}, nil
}
