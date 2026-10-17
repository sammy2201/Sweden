SHELL := /bin/bash

BACKEND_DIR := backend
FRONTEND_DIR := frontend
BACKEND_PROFILE ?= http
FRONTEND_HOST ?= 0.0.0.0
FRONTEND_PORT ?= 4200
ENV_FILE ?= .env
ENV_RUN := set -e; if [ ! -f "../$(ENV_FILE)" ]; then printf 'Required environment file ../%s is missing\n' "$(ENV_FILE)" >&2; exit 1; fi; set -a; source "../$(ENV_FILE)"; set +a;
.DEFAULT_GOAL := help
.PHONY: help all install restore build run dev watch \
	db_up db_down db_restart db_status db_logs db_shell \
	backend_restore backend_build backend_run backend_watch \
	frontend_install frontend_build frontend_publish frontend_start frontend_serve frontend_watch \
	db_migrate db_migrate_add db_migrate_remove

help: ## Show available make commands
	@awk 'BEGIN {FS = ":.*##"; printf "\nCommands:\n"} /^[a-zA-Z0-9_-]+:.*##/ {printf "  %-18s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@printf "\nCommon flow:\n"
	@printf "  make db_up   # start local Homebrew PostgreSQL\n"
	@printf "  make dev            # run backend + frontend with native PostgreSQL\n"
	@printf "  make build          # build backend + frontend\n\n"

all: build ## Build everything

install: restore ## Restore backend packages and install frontend packages

restore: backend_restore frontend_install ## Restore/install all dependencies

build: backend_build frontend_build ## Build backend and frontend

run: dev ## Alias for dev

dev: db_up ## Start native database, backend, and frontend together
	$(MAKE) -j2 backend_run frontend_start

watch: db_up ## Start native database, backend watch, and frontend dev server
	$(MAKE) -j2 backend_watch frontend_start

db_up: ## Start PostgreSQL 18 through Homebrew services
	@brew services start postgresql@18

db_down: ## Stop PostgreSQL 18 through Homebrew services
	@brew services stop postgresql@18

db_restart: ## Restart PostgreSQL 18 through Homebrew services
	@brew services restart postgresql@18

db_status: ## Show PostgreSQL 18 Homebrew service status
	@brew services info postgresql@18

db_logs: ## Follow the PostgreSQL 18 Homebrew service log
	@tail -f "$(HOME)/Library/Logs/Homebrew/postgresql@18.log"

db_shell: ## Open psql on native PostgreSQL
	cd $(BACKEND_DIR) && $(ENV_RUN) psql -h localhost -p 5434 -U sweden_start -d sweden_start

backend_restore: ## Restore .NET backend packages
	cd $(BACKEND_DIR) && dotnet restore

backend_build: ## Build .NET backend
	cd $(BACKEND_DIR) && dotnet build

db_migrate: ## Apply EF Core database migrations to the target database
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet ef database update

db_migrate_add: ## Add a new EF Core migration. Use MIGRATION_NAME=YourName
	@if [[ ! "$$MIGRATION_NAME" =~ ^[A-Za-z][A-Za-z0-9_]*$$ ]]; then \
		printf 'Set MIGRATION_NAME to a name beginning with a letter and containing only letters, numbers, or underscores.\n' >&2; \
		exit 2; \
	fi
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet ef migrations add "$$MIGRATION_NAME"

db_migrate_remove: ## Remove the last EF Core migration
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet ef migrations remove

backend_run: ## Run .NET backend on the configured launch profile
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet run --launch-profile $(BACKEND_PROFILE)

backend_watch: ## Run .NET backend with hot reload
	cd $(BACKEND_DIR) && $(ENV_RUN) dotnet watch run --launch-profile $(BACKEND_PROFILE)

frontend_install: ## Install frontend npm packages
	cd $(FRONTEND_DIR) && npm install

frontend_build: ## Build Angular frontend
	cd $(FRONTEND_DIR) && npm run build

frontend_publish: ## Build Angular and copy it to backend/wwwroot
	cd $(FRONTEND_DIR) && npm run build
	rm -rf $(BACKEND_DIR)/wwwroot/*
	cp -R $(FRONTEND_DIR)/dist/sweden-start/browser/* $(BACKEND_DIR)/wwwroot/
	@printf "Frontend published to backend/wwwroot\n"
	
frontend_start: ## Run Angular dev server
	cd $(FRONTEND_DIR) && npm start -- --host $(FRONTEND_HOST) --port $(FRONTEND_PORT)

frontend_serve: frontend_start ## Alias for frontend_start

frontend_watch: ## Build Angular frontend in watch mode
	cd $(FRONTEND_DIR) && npm run watch
