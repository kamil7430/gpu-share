package model

type DeviceState int

const (
	Unavailable DeviceState = iota
	Available
	Rented
	Reported
)
