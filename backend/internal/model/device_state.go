package model

type DeviceState string

const (
	Unavailable DeviceState = "UNAVAILABLE"
	Available   DeviceState = "AVAILABLE"
	Rented      DeviceState = "RENTED"
	Reported    DeviceState = "REPORTED"
)
