echo off

set "$(git-branch)=dev1"
echo %$(git-branch)%

echo %$(TargetPath)%
echo $(TargetPath)
echo %TargetPath%

echo %TargetPath%
echo %(TargetPath)

call git rev-parse --abbrev-ref HEAD > "D:\dev\git\tentakel\git-branch"

