@echo off
REM MCP Unity Server - Быстрые команды
REM Использование: mcp-unity.bat [command]

set MCP_URL=http://127.0.0.1:8080/mcp
set MCP_HEADERS=Content-Type: application/json
set MCP_ACCEPT=Accept: application/json, text/event-stream

if "%1"=="init" goto init
if "%1"=="play" goto play
if "%1"=="stop" goto stop
if "%1"=="pause" goto pause
if "%1"=="list" goto list
if "%1"=="console" goto console
if "%1"=="" goto help

:init
echo Инициализация MCP сервера...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2024-11-05\",\"capabilities\":{},\"clientInfo\":{\"name\":\"qwen\",\"version\":\"1.0.0\"}},\"id\":1}"
goto end

:play
echo Запуск игры (Play Mode)...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"tools/call\",\"params\":{\"name\":\"manage_editor\",\"arguments\":{\"action\":\"play\"}},\"id\":2}"
goto end

:stop
echo Остановка игры...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"tools/call\",\"params\":{\"name\":\"manage_editor\",\"arguments\":{\"action\":\"stop\"}},\"id\":2}"
goto end

:pause
echo Пауза...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"tools/call\",\"params\":{\"name\":\"manage_editor\",\"arguments\":{\"action\":\"pause\"}},\"id\":2}"
goto end

:list
echo Список доступных инструментов...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"tools/list\",\"id\":2}"
goto end

:console
echo Чтение консоли Unity...
curl -s -X POST %MCP_URL% ^
  -H "%MCP_HEADERS%" -H "%MCP_ACCEPT%" ^
  -d "{\"jsonrpc\":\"2.0\",\"method\":\"tools/call\",\"params\":{\"name\":\"read_console\",\"arguments\":{}},\"id\":2}"
goto end

:help
echo ================================
echo MCP Unity Server - Команды
echo ================================
echo   mcp-unity init     - Инициализация сервера
echo   mcp-unity play     - Запуск игры
echo   mcp-unity stop     - Остановка игры
echo   mcp-unity pause    - Пауза
echo   mcp-unity list     - Список инструментов
echo   mcp-unity console  - Чтение консоли Unity
echo ================================
goto end

:end
