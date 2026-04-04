package service

import (
	"context"

	"github.com/kamil7430/gpu-share/gpu/coordinator/api"
)

type JobsService struct {
	as *AgentService
}

func NewJobsService(as *AgentService) *JobsService {
	return &JobsService{as}
}

func (js *JobsService) ScheduleTask(ctx context.Context, r *api.ScheduleTaskReq) (api.ScheduleTaskRes, error) {
	jobId, err := js.as.SendTask(string(r.DeviceId), r.Resources.VRamMb)
	if err != nil {
		// TODO: probably should return sf else
		return nil, err
	}

	return &api.ScheduleTaskCreated{
		JobId: jobId,
	}, nil
}
