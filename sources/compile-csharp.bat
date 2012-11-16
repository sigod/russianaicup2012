set name=MyStrategy

if not exist %name%.cs (
    echo Unable to find %name%.cs > compilation.log
    exit 1
)

del /F /Q %name%.mono-exe

SET FILES=

for %%i in (*.cs) do (
    call concatenate %%i
)

for %%i in (Model\*.cs) do (
    call concatenate %%i
)

for %%i in (Properties\*.cs) do (
    call concatenate %%i
)

call csc -define:ONLINE_JUDGE -o+ -out:%name%.exe %FILES% 2>compilation.log
