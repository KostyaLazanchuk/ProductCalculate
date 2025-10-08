using ThreadWar.Controller;
using ThreadWar.Core;
using ThreadWar.Model;
using ThreadWar.View;

Console.CursorVisible = false;
Console.Clear();

var gameState = new GameState
{
    Width = Math.Max(60, Console.WindowWidth),
    Height = Math.Max(20, Console.WindowHeight),
    CannonX = Math.Max(60, Console.WindowWidth) / 2
};

var view = new ConsoleView(gameState);
view.ShowSplash();

var engine = new GameEngine(gameState);

var controller = new GameController(gameState, engine, view);

engine.StartBackgroundSystems();

controller.Run();

view.ShowGameOver();

