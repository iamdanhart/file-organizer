dir := "/tmp/file-organizer-test"

_setup:
    #!/usr/bin/env zsh
    if [[ "{{dir}}" != "/tmp/file-organizer-test" ]]; then
        echo "Error: refusing to recreate a non-fixture directory"
        exit 1
    fi
    rm -rf {{dir}}
    mkdir -p {{dir}}

run *args:
    dotnet run --project file-organizer.Cli -- {{args}}

test:
    dotnet test file-organizer.Core.Tests

build:
    dotnet build

dry-run-test: test-data
    just run {{dir}} --dry-run
copy-test: test-data
    just run {{dir}} --copy
execute-test: test-data
    just run {{dir}} --execute
test-data: _setup
    just run {{dir}} --test-data --config