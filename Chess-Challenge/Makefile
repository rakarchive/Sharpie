ARCHITECTURE = linux-x64

ifeq ($(OS), Windows_NT)
	ARCHITECTURE = win-x64
endif

build:
	dotnet publish -r $(ARCHITECTURE) -c Release /p:PublishSingleFile=true --self-contained true -o ./

.PHONY: build