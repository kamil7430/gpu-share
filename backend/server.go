package main

import (
	"net/http"

	"gorm.io/gorm"
)

func NewServer(db *gorm.DB) http.Handler {
	mux := http.NewServeMux()

	//eventRepo := repository.NewEventRepository(db)
	//teamRepo := repository.NewTeamRepository(db)
	//
	//eventHandler := handler.NewEventHandler(eventRepo)
	//teamHandler := handler.NewTeamHandler(teamRepo)

	//mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
	//	if err := db.Ping(); err != nil {
	//		http.Error(w, "DB not reachable", 500)
	//		return
	//	}
	//	w.Write([]byte("OK"))
	//})

	//mux.HandleFunc("GET /events", eventHandler.GetEvents)
	//mux.HandleFunc("GET /events/{id}", eventHandler.GetEvent)
	//mux.HandleFunc("POST /events", eventHandler.CreateEvent)
	//mux.HandleFunc("GET /teams", teamHandler.GetTeams)
	//mux.HandleFunc("GET /teams/{id}", teamHandler.GetTeam)
	//mux.HandleFunc("POST /teams", teamHandler.CreateTeam)

	fs := http.FileServer(http.Dir("/app/frontend"))
	mux.Handle("/", fs)

	return mux
}
