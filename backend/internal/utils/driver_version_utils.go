package utils

import (
	"cmp"
	"errors"
	"fmt"
	"strings"
)

type DriverVersion struct {
	Major int
	Minor int
}

func NewDriverVersion(major int, minor int) (*DriverVersion, error) {
	if major < 0 || minor < 0 {
		return nil, errors.New("invalid version")
	}

	return &DriverVersion{major, minor}, nil
}

func DriverVersionFromString(driverVersion string) (*DriverVersion, error) {
	var dv DriverVersion
	n, err := fmt.Fscanf(strings.NewReader(driverVersion), "%d.%d", &dv.Major, &dv.Minor)
	if err != nil {
		return nil, err
	}
	if n != 2 {
		return nil, errors.New("invalid version")
	}
	return &dv, nil
}

func (dv *DriverVersion) String() string {
	return fmt.Sprintf("%d.%d", dv.Major, dv.Minor)
}

func (dv *DriverVersion) Compare(dv2 *DriverVersion) int {
	if dv.Major > dv2.Major {
		return 1
	}
	if dv.Major == dv2.Major {
		return cmp.Compare(dv.Minor, dv2.Minor)
	}
	return -1
}
