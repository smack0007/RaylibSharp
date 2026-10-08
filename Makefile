
TARGETS_WITH_ARGS := build-example run-example
TARGET_NAME       := $(firstword $(MAKECMDGOALS))
ifeq ($(filter $(TARGET_NAME),$(TARGETS_WITH_ARGS)),$(TARGET_NAME))
  RUN_ARGS := $(wordlist 2,$(words $(MAKECMDGOALS)),$(MAKECMDGOALS))
  $(eval $(RUN_ARGS):;@:)
endif

PROJECT_FILES := $(shell find . -name "*.csproj" -type f)
PROJECT_DIRS := $(shell find . -name "*.csproj" -type f | xargs dirname)

default: build

.PHONY: build
build:
	cd ./src && dotnet build

.PHONY: build-all
build-all:
	@for dir in $(PROJECT_DIRS); do \
		pushd $$dir > /dev/null; \
		dotnet build; \
		popd > /dev/null; \
	done

.PHONY: codegen
codegen:
	cd ./tools/codegen && dotnet run

.PHONY: clean
clean:
	@for dir in $(PROJECT_DIRS); do \
		pushd $$dir > /dev/null; \
		dotnet clean -v q; \
		popd > /dev/null; \
	done

.PHONY: build-example
build-example:
	cd ./examples/$(RUN_ARGS) && dotnet build

.PHONY: run-example
run-example:
	cd ./examples/$(RUN_ARGS) && dotnet run
