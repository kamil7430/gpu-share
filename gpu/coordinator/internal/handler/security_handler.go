package handler

import (
	"context"
	"crypto/subtle"

	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/api"
	"github.com/ogen-go/ogen/ogenerrors"
)

type SecurityHandler struct {
	apiKey string
}

func NewSecurityHandler(apiKey string) *SecurityHandler {
	return &SecurityHandler{apiKey}
}

func (s *SecurityHandler) HandleCoordinatorApiKey(ctx context.Context, operationName api.OperationName, key api.CoordinatorApiKey) (context.Context, error) {
	got := key.APIKey
	want := s.apiKey

	if subtle.ConstantTimeCompare([]byte(got), []byte(want)) == 0 {
		return ctx, ogenerrors.ErrSecurityRequirementIsNotSatisfied
	}

	return ctx, nil
}
