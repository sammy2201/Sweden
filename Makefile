SHELL := /bin/bash

BACKEND_DIR := backend
FRONTEND_DIR := frontend
BACKEND_PROFILE ?= http
FRONTEND_HOST ?= 0.0.0.0
FRONTEND_PORT ?= 4200
COMPOSE := docker compose
ENV_FILE ?= .env
ENV_RUN := set -a; [ ! -f ../$(ENV_FILE) ] || source ../$(ENV_FILE); set +a;

.DEFAULT_GOAL := help
.PHONY: help all install restore build run dev watch \
	db_up db_down db_restart db_status db_logs db_shell \
	backend_restore backend_build backend_run backend_watch \
	frontend_install frontend_build frontend_start frontend_serve frontend_watch

help: ## Show available make commands
	@awk 'BEGIN {FS = ":.*##"; printf "\nCommands:\n"} /^[a-zA-Z0-9_-]+:.*##/ {printf "  %-18s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@printf "\nCommon flow:\n"
	@printf "  make db_up          # start PostgreSQL in Docker\n"
	@printf "  make dev            # run backend + frontend with database\n"
	@printf "  make build          # build backend + frontend\n\n"

all: build ## Build everything

install: restore ## Restore backend packages and install frontend packages

restore: backend_restore frontend_install ## Restore/install all dependencies

build: backend_build frontend_build ## Build backend and frontend

run: dev ## Alias for dev

dev: db_up ## Start database, backend, and frontend together
	$(MAKE) -j2 backend_run frontend_start

watch: db_up ## Start database, backend watch, and frontend dev server
	$(MAKE) -j2 backend_watch frontend_start

db_up: ## Start PostgreSQL container
	$(COMPOSE) up -d postgres

db_down: ## Stop PostgreSQL container without deleting data
	$(COMPOSE) down

db_restart: ## Restart PostgreSQL container
	$(COMPOSE) restart postgres

db_status: ## Show PostgreSQL container status
	$(COMPOSE) ps postgres

db_logs: ## Follow PostgreSQL logs
	$(COMPOSE) logs -f postgres

db_shell: ## Open psql inside the PostgreSQL container
	$(COMPOSE) exec postgres psql -U sweden_start -d sweden_start

backend_restore: ## Restore .NET backend packages
	cd $(BACKEND_DIR) && dotnet restore

backend_build: ## Build .NET backend
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet build

backend_run: ## Run .NET backend on the configured launch profile
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet run --launch-profile $(BACKEND_PROFILE)

backend_watch: ## Run .NET backend with hot reload
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet watch run --launch-profile $(BACKEND_PROFILE)

frontend_install: ## Install frontend npm packages
	cd $(FRONTEND_DIR) && npm install

frontend_build: ## Build Angular frontend
	cd $(FRONTEND_DIR) && npm run build

frontend_start: ## Run Angular dev server
	cd $(FRONTEND_DIR) && npm start -- --host $(FRONTEND_HOST) --port $(FRONTEND_PORT)

frontend_serve: frontend_start ## Alias for frontend_start

frontend_watch: ## Build Angular frontend in watch mode
	cd $(FRONTEND_DIR) && npm run watch