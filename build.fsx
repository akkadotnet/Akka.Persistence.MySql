#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli
open Fake.DocFxHelper

// Variables
let configuration = "Release"

// Read release notes and version
let solutionFile = FindFirstMatchingFile "*.sln" (__SOURCE_DIRECTORY__ @@ "src") // dynamically look up the solution
let buildNumber = environVarOrDefault "BUILD_NUMBER" "0"
let hasTeamCity = (not (buildNumber = "0")) // check if we have the TeamCity environment variable for build # set
let preReleaseVersionSuffix = "beta" + (if (not (buildNumber = "0")) then (buildNumber) else DateTime.UtcNow.Ticks.ToString())

let releaseNotes =
    File.ReadLines (__SOURCE_DIRECTORY__ @@ "RELEASE_NOTES.md")
    |> ReleaseNotesHelper.parseReleaseNotes

let versionFromReleaseNotes =
    match releaseNotes.SemVer.PreRelease with
    | Some r -> r.Origin
    | None -> ""

let versionSuffix = 
    match (getBuildParam "nugetprerelease") with
    | "dev" -> preReleaseVersionSuffix
    | "" -> versionFromReleaseNotes
    | str -> str

// Directories
let output = __SOURCE_DIRECTORY__  @@ "build"
let outputTests = output @@ "tests"
let outputBinaries = output @@ "binaries"
let outputNuGet = output @@ "nuget"

Target "Clean" (fun _ ->
    CleanDir output
    CleanDir outputTests
    CleanDir outputBinaries
    CleanDir outputNuGet

    CleanDirs !! "./**/bin"
    CleanDirs !! "./**/obj"
)

Target "RestorePackages" (fun _ ->
    DotNetCli.Restore
        (fun p -> 
            { p with
                Project = "./src/Akka.Persistence.MySql.sln"
                NoCache = false })
)

Target "Build" (fun _ ->
    DotNetCli.Build
        (fun p -> 
            { p with
                Project = solutionFile
                Configuration = configuration })
)

//--------------------------------------------------------------------------------
// Tests targets 
//--------------------------------------------------------------------------------
module internal ResultHandling =
    let (|OK|Failure|) = function
        | 0 -> OK
        | x -> Failure x

    let buildErrorMessage = function
        | OK -> None
        | Failure errorCode ->
            Some (sprintf "xUnit2 reported an error (Error Code %d)" errorCode)

    let failBuildWithMessage = function
        | DontFailBuild -> traceError
        | _ -> (fun m -> raise(FailedTestsException m))

    let failBuildIfXUnitReportedError errorLevel =
        buildErrorMessage
        >> Option.iter (failBuildWithMessage errorLevel)

Target "RunTests" (fun _ ->
    let projects = 
        match (isWindows) with 
        | true -> !! "./src/**/*.Tests.csproj"
        | _ -> !! "./src/**/*.Tests.csproj" // if you need to filter specs for Linux vs. Windows, do it here

    let runSingleProject project =
        let arguments =
            match (hasTeamCity) with
            | true -> (sprintf "test -c Release --no-build --logger:trx --logger:\"console;verbosity=normal\" --results-directory %s -- -parallel none -teamcity" (outputTests))
            | false -> (sprintf "test -c Release --no-build --logger:trx --logger:\"console;verbosity=normal\" --results-directory %s -- -parallel none" (outputTests))

        let result = ExecProcess(fun info ->
            info.FileName <- "dotnet"
            info.WorkingDirectory <- (Directory.GetParent project).FullName
            info.Arguments <- arguments) (TimeSpan.FromMinutes 30.0) 
        
        ResultHandling.failBuildIfXUnitReportedError TestRunnerErrorLevel.Error result  

    projects |> Seq.iter (log)
    projects |> Seq.iter (runSingleProject)
)

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------

Target "CreateNuget" (fun _ ->
    let envBuildNumber = environVarOrDefault "APPVEYOR_BUILD_NUMBER" "0"
    let branch =  environVarOrDefault "APPVEYOR_REPO_BRANCH" ""
    let versionSuffix = if branch.Equals("dev") then (sprintf "beta%s" envBuildNumber) else ""

    let projects = !! "src/**/Akka.Persistence.MySql.csproj"

    let runSingleProject project =
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = project
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols"]
                    VersionSuffix = versionSuffix
                    OutputPath = outputNuGet })

    projects |> Seq.iter (runSingleProject)
)

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "BuildRelease" DoNothing
Target "All" DoNothing
Target "Nuget" DoNothing

// build dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "BuildRelease"

// tests dependencies
"Build" ==> "RunTests"

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget" ==> "Nuget"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"Nuget" ==> "All"

RunTargetOrDefault "Help"