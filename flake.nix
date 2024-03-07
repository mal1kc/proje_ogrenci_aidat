{
  # templated from templates#dotnet
  description = "Student Payment Managment System in .NetCore8 MVC";
  inputs = {
    nixpkgs.url = "nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };
  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        system="x86_64-linux";
        pkgs = import nixpkgs { inherit system; };
        projectFile = "./OgrenciAidatSistemi/OgrenciAidatSistemi.csproj";
        pname = "dotnet-ogrenciaidatsistemi";
        dotnet-sdk = pkgs.dotnet-sdk_8;
        dotnet-runtime = pkgs.dotnet-runtime_8;
        version = "0.0.1";
        # dotnetSixTool = toolName: toolVersion: sha256:
        #   pkgs.stdenvNoCC.mkDerivation rec {
        #     name = toolName;
        #     version = toolVersion;
        #     nativeBuildInputs = [ pkgs.makeWrapper ];
        #     src = pkgs.fetchNuGet {
        #       pname = name;
        #       version = version;
        #       sha256 = sha256;
        #       installPhase =
        #         "mkdir -p $out/bin && cp -r tools/net6.0/any/* $out/bin";
        #     };
        #     installPhase = ''
        #       runHook preInstall
        #       mkdir -p "$out/lib"
        #       cp -r ./bin/* "$out/lib"
        #       makeWrapper "${dotnet-runtime}/bin/dotnet" "$out/bin/${name}" --add-flags "$out/lib/${name}.dll"
        #       runHook postInstall
        #     '';
        #   };
      in {
        packages = {
          fetchDeps = let
            # flags = [ ];
            runtimeIds =
              map (system: pkgs.dotnetCorePackages.systemToDotnetRid system)
              dotnet-sdk.meta.platforms;
          in pkgs.writeShellScriptBin "fetch-${pname}-deps" (builtins.readFile
            (pkgs.substituteAll {
              src = ./nix/fetchDeps.sh;
              pname = pname;
              binPath = pkgs.lib.makeBinPath [
                pkgs.coreutils
                dotnet-sdk
                (pkgs.nuget-to-nix.override { inherit dotnet-sdk; })
              ];
              projectFiles = toString (pkgs.lib.toList projectFile);
              # testProjectFiles = toString (pkgs.lib.toList testProjectFile);
              rids = pkgs.lib.concatStringsSep ''" "'' runtimeIds;
              packages = dotnet-sdk.packages;
              storeSrc = pkgs.srcOnly {
                src = ./.;
                pname = pname;
                version = version;
              };
            }));
          default = pkgs.buildDotnetModule {
            pname = "OgrenciAidatSistemi";
            version = version;
            src = ./.;
            projectFile = projectFile;
            nugetDeps = ./nix/deps.nix;
            doCheck = true;
            dotnet-sdk = dotnet-sdk;
            dotnet-runtime = dotnet-runtime;
            DOTNET_ROOT = "${dotnet-sdk}";
            DOTNET_BIN = "${dotnet-sdk}/bin/dotnet";
          };
        };
        devShells = {
          default =
            pkgs.mkShell {
                buildInputs = [ dotnet-sdk pkgs.git pkgs.csharp-ls pkgs.gawk pkgs.tmux];
                DOTNET_ROOT = "${dotnet-sdk}";
                DOTNET_BIN = "${dotnet-sdk}/bin/dotnet";
                shellHook = ''
                    tmux_session_path="/tmp/tmux-$(whoami)-sch_proje"
                    if [ $TMUX ] ; then
                        printf  "already in tmux , continue to shell\n"
                        $SHELL
                        printf "exited shell\n"
                    else
                        if [[ ! -S $tmux_session_path ]]; then
                            printf  "tmux session not exits created new\n"
                            tmux -S $tmux_session_path new-session -A
                        else
                            printf  "attaching tmux session\n"
                            printf  "SHell=%s\n" "$SHELL"
                            tmux -S $tmux_session_path attach || tmux -S $tmux_session_path new-session -A $SHELL
                        fi
                    fi
                    exit
                    '';
            };
        };
      });
}
