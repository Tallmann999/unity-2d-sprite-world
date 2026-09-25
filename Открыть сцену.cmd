@echo off
where unity >nul 2>nul
if errorlevel 1 (
 echo Open this folder in Unity Hub, then open Assets/Scenes/FlatDungeon.unity.
 pause
 exit /b 1
)
unity open "%~dp0." --args "-executeMethod BuildFlatDungeon.OpenPreview"
