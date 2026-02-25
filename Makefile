CONFIGURATION   := Debug
V               ?= 0

include eng/scripts/msbuild.mk

all:
	$(MSBUILD) $(MSBUILD_FLAGS) Xamarin.MacDev.sln

clean:
	-$(MSBUILD) $(MSBUILD_FLAGS) /t:Clean Xamarin.MacDev.sln

run-all-tests:
	dotnet test -l "console;verbosity=detailed" -l trx \
		UnitTests/UnitTests.csproj

pack:
	dotnet pack Xamarin.MacDev/Xamarin.MacDev.csproj $(MSBUILD_FLAGS)
