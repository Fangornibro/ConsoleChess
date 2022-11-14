using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
namespace Chess
{
    //Структура для нашої фiгури side(бiла/чорна), type(королева, король i т.д.), curx, cury(позицiя), mark(знак яким позначається фiгура), isselected(чи цю фiгуру видiлив користувач), FirstPawnMv(тiльки для пiшки, чи це її перший рух)
    struct Figure
    {
        public string side { get; set; }
        public string type { get; set; }
        public int curx { get; set; }
        public int cury { get; set; }
        public char mark { get; set; }
        public bool isselected { get; set; }

        public bool FirstPawnMv { get; set; }

        public static implicit operator Figure((string, string, int, int, char, bool, bool) value) =>
              new Figure { side = value.Item1, type = value.Item2, curx = value.Item3, cury = value.Item4, mark = value.Item5, isselected = value.Item6, FirstPawnMv = value.Item7 };

    }
    //Структура для кординат
    struct cordinates
    {
        public int x { get; set; }
        public int y { get; set; }

        public static implicit operator cordinates((int, int) value) =>
              new cordinates { x = value.Item1, y = value.Item2 };

    }
    class Program
    {

        static public bool IsSelectedGlobal = false;
        static public string turn = "White";
        static public bool GameOver = false;
        //Оновлення доски(виконується при усiх дiях гравцiв). Повнiстю вiдчищає доску, та вiдмальовує її заново.
        static void UpdateChess(List<Figure> curFigures)
        {

            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Console.SetCursorPosition(i, j);
                    if (i % 2 == 0 && j % 2 == 0 || i % 2 != 0 && j % 2 != 0 || i == 0 || i == 9 || j == 0 || j == 9)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    Console.Write(" ");
                }
            }

            for (int i = 0; i < curFigures.Count; i++)
            {
                Console.SetCursorPosition(curFigures[i].curx, curFigures[i].cury);
                if (curFigures[i].isselected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    if (curFigures[i].side == "White")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (curFigures[i].side == "Black")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

                }
                if (curFigures[i].curx % 2 == 0 && curFigures[i].cury % 2 == 0 || curFigures[i].curx % 2 != 0 && curFigures[i].cury % 2 != 0)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(curFigures[i].mark);
            }
            Console.SetCursorPosition(left, top);
        }
        //Розташування усiх фiгур на початку гри
        static List<Figure> FiguresSpawn()
        {
            List<Figure> curFigures = new List<Figure>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (j == 2 && i != 0 && i != 9)
                    {
                        Figure Fig = ("Black", "Pawn", i, j, '6', false, true);
                        curFigures.Add(Fig);
                    }
                    if (j == 1 && (i == 1 || i == 8))
                    {
                        Figure Fig = ("Black", "Bishop", i, j, '5', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 1 && (i == 2 || i == 7))
                    {
                        Figure Fig = ("Black", "Knight", i, j, '4', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 1 && (i == 3 || i == 6))
                    {
                        Figure Fig = ("Black", "Rook", i, j, '3', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 1 && i == 4)
                    {
                        Figure Fig = ("Black", "King", i, j, '1', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 1 && i == 5)
                    {
                        Figure Fig = ("Black", "Queen", i, j, '2', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 7 && i != 0 && i != 9)
                    {
                        Figure Fig = ("White", "Pawn", i, j, '6', false, true);
                        curFigures.Add(Fig);
                    }
                    if (j == 8 && (i == 1 || i == 8))
                    {
                        Figure Fig = ("White", "Bishop", i, j, '5', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 8 && (i == 2 || i == 7))
                    {
                        Figure Fig = ("White", "Knight", i, j, '4', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 8 && (i == 3 || i == 6))
                    {
                        Figure Fig = ("White", "Rook", i, j, '3', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 8 && i == 4)
                    {
                        Figure Fig = ("White", "King", i, j, '1', false, false);
                        curFigures.Add(Fig);
                    }
                    if (j == 8 && i == 5)
                    {
                        Figure Fig = ("White", "Queen", i, j, '2', false, false);
                        curFigures.Add(Fig);
                    }
                }
            }
            UpdateChess(curFigures);
            return curFigures;
        }
        //Операцiї з клавiатурою. Пересування вказiвника й видiлення фiгур(а також звуки при видiленнi)
        static void KeyboardOperation(ConsoleKey key, List<Figure> curFigures)
        {
            string fileName = "ChessSound.wav";
            string path = Path.Combine(Environment.CurrentDirectory, @"Sounds\", fileName);
            SoundPlayer sound = new SoundPlayer(path);
            switch (key)
            {
                case ConsoleKey.A:
                    if (Console.CursorLeft != 1)
                    {
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    break;
                case ConsoleKey.D:
                    if (Console.CursorLeft != 8)
                    {
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                    break;
                case ConsoleKey.S:
                    if (Console.CursorTop != 8)
                    {
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
                    }
                    break;
                case ConsoleKey.W:
                    if (Console.CursorTop != 1)
                    {
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    }
                    break;
                case ConsoleKey.Enter:
                    sound.Play();
                    FigureMovement(curFigures);
                    break;
                case ConsoleKey.Spacebar:
                    sound.Play();
                    FigureMovement(curFigures);
                    break;
            }
        }


        static void FigureMovement(List<Figure> curFigures)
        {
            //Якщо жодна фiгура не видiлена
            if (IsSelectedGlobal == false)
            {
                //Перевiряємо кожну фiгуру на столi. Якщо користувач натиснув enter/space на фiгурi(сторона якої зараз ходить), то ця фiгура видiляється
                for (int i = 0; i < curFigures.Count; i++)
                {
                    if (curFigures[i].curx == Console.CursorLeft && curFigures[i].cury == Console.CursorTop && curFigures[i].side == turn)
                    {
                        IsSelectedGlobal = true;
                        Figure slFigure = curFigures[i];
                        slFigure.isselected = true;
                        curFigures[i] = slFigure;
                        UpdateChess(curFigures);
                    }
                }
            }
            //Якщо якась фiгура видiлена
            else
            {
                IsSelectedGlobal = false;
                //Перемiщаємо фiгуру на позицiю курсора
                for (int i = 0; i < curFigures.Count; i++)
                {
                    if (curFigures[i].isselected)
                    {
                        move(curFigures, i);
                    }
                    Figure UnSelect = curFigures[i];
                    UnSelect.isselected = false;
                    curFigures[i] = UnSelect;
                }
                UpdateChess(curFigures);
            }
        }
        //Головна функцiя, що вiдповiдає за рух фiгури кожного типу(а також за їх атаку)
        static void move(List<Figure> curFigures, int id)
        {
            Figure mvFigure = curFigures[id];
            mvFigure.curx = Console.CursorLeft;
            mvFigure.cury = Console.CursorTop;
            mvFigure.FirstPawnMv = false;

            switch (curFigures[id].type)
            {
                case "Pawn":
                    if ((curFigures[id].FirstPawnMv && Math.Abs(curFigures[id].cury - mvFigure.cury) > 2) || (!curFigures[id].FirstPawnMv && Math.Abs(curFigures[id].cury - mvFigure.cury) > 1) || (curFigures[id].cury == mvFigure.cury) && (curFigures[id].curx == mvFigure.curx))
                    {
                        return;
                    }
                    if (turn == "Black")
                    {
                        if (curFigures[id].cury - mvFigure.cury > 0)
                        {
                            return;
                        }
                        if (curFigures[id].cury + 1 == mvFigure.cury && (curFigures[id].curx + 1 == mvFigure.curx || curFigures[id].curx - 1 == mvFigure.curx))
                        {
                            for (int j = 0; j < curFigures.Count; j++)
                            {
                                if (curFigures[j].side != curFigures[id].side && curFigures[j].cury == mvFigure.cury && curFigures[j].curx == mvFigure.curx)
                                {
                                    remove(curFigures, j);
                                    goto M1;
                                }
                            }
                        }
                        if (curFigures[id].curx != mvFigure.curx)
                        {
                            return;
                        }
                    M1:
                        for (int i = 0; i < curFigures.Count; i++)
                        {
                            if (curFigures[i].curx == mvFigure.curx && curFigures[id].cury < curFigures[i].cury && curFigures[i].cury <= mvFigure.cury)
                            {
                                return;
                            }
                        }
                        curFigures[id] = mvFigure;
                        SwitchTurn();
                        if (curFigures[id].cury == 8)
                        {
                            mvFigure = curFigures[id];
                            mvFigure.mark = '2';
                            mvFigure.type = "Queen";
                            curFigures[id] = mvFigure;
                        }
                    }
                    else if (turn == "White")
                    {
                        if (curFigures[id].cury - mvFigure.cury < 0)
                        {
                            return;
                        }
                        if (curFigures[id].cury - 1 == mvFigure.cury && (curFigures[id].curx + 1 == mvFigure.curx || curFigures[id].curx - 1 == mvFigure.curx))
                        {
                            for (int j = 0; j < curFigures.Count; j++)
                            {
                                if (curFigures[j].side != curFigures[id].side && curFigures[j].cury == mvFigure.cury && curFigures[j].curx == mvFigure.curx)
                                {
                                    remove(curFigures, j);
                                    goto M2;
                                }
                            }
                        }
                        if (curFigures[id].curx != mvFigure.curx)
                        {
                            return;
                        }
                    M2:
                        for (int i = 0; i < curFigures.Count; i++)
                        {
                            if (curFigures[i].curx == mvFigure.curx && curFigures[id].cury > curFigures[i].cury && curFigures[i].cury >= mvFigure.cury)
                            {
                                return;
                            }
                        }
                        curFigures[id] = mvFigure;
                        SwitchTurn();
                        if (curFigures[id].cury == 1)
                        {
                            mvFigure = curFigures[id];
                            mvFigure.mark = '2';
                            mvFigure.type = "Queen";
                            curFigures[id] = mvFigure;
                        }
                    }
                    break;
                case "Bishop":
                    if (!(curFigures[id].curx != mvFigure.curx && curFigures[id].cury == mvFigure.cury) && !(curFigures[id].curx == mvFigure.curx && curFigures[id].cury != mvFigure.cury))
                    {
                        return;
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].side == mvFigure.side && (((curFigures[i].curx == mvFigure.curx && curFigures[id].cury > curFigures[i].cury && curFigures[i].cury >= mvFigure.cury) || (curFigures[i].curx == mvFigure.curx && curFigures[id].cury < curFigures[i].cury && curFigures[i].cury <= mvFigure.cury)) || ((curFigures[i].cury == mvFigure.cury && curFigures[id].curx > curFigures[i].curx && curFigures[i].curx >= mvFigure.curx) || (curFigures[i].cury == mvFigure.cury && curFigures[id].curx < curFigures[i].curx && curFigures[i].curx <= mvFigure.curx))))
                        {
                            return;
                        }
                        if (curFigures[i].side != mvFigure.side && (((curFigures[i].curx == mvFigure.curx && curFigures[id].cury > curFigures[i].cury && curFigures[i].cury > mvFigure.cury) || (curFigures[i].curx == mvFigure.curx && curFigures[id].cury < curFigures[i].cury && curFigures[i].cury < mvFigure.cury)) || ((curFigures[i].cury == mvFigure.cury && curFigures[id].curx > curFigures[i].curx && curFigures[i].curx > mvFigure.curx) || (curFigures[i].cury == mvFigure.cury && curFigures[id].curx < curFigures[i].curx && curFigures[i].curx < mvFigure.curx))))
                        {
                            return;
                        }
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].cury == mvFigure.cury && curFigures[i].curx == mvFigure.curx)
                        {
                            remove(curFigures, i);
                        }
                    }
                    curFigures[id] = mvFigure;
                    SwitchTurn();
                    break;
                case "Knight":
                    if (!((curFigures[id].curx + 2 == mvFigure.curx && curFigures[id].cury + 1 == mvFigure.cury) || (curFigures[id].curx + 2 == mvFigure.curx && curFigures[id].cury - 1 == mvFigure.cury) || (curFigures[id].curx - 2 == mvFigure.curx && curFigures[id].cury + 1 == mvFigure.cury) || (curFigures[id].curx - 2 == mvFigure.curx && curFigures[id].cury - 1 == mvFigure.cury) || (curFigures[id].curx + 1 == mvFigure.curx && curFigures[id].cury + 2 == mvFigure.cury) || (curFigures[id].curx + 1 == mvFigure.curx && curFigures[id].cury - 2 == mvFigure.cury) || (curFigures[id].curx - 1 == mvFigure.curx && curFigures[id].cury + 2 == mvFigure.cury) || (curFigures[id].curx - 1 == mvFigure.curx && curFigures[id].cury - 2 == mvFigure.cury)))
                    {
                        return;
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].side != mvFigure.side && curFigures[i].cury == mvFigure.cury && curFigures[i].curx == mvFigure.curx)
                        {
                            remove(curFigures, i);
                            goto M3;
                        }
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {
                        if (curFigures[i].curx == mvFigure.curx && curFigures[i].cury == mvFigure.cury)
                        {
                            return;
                        }
                    }
                M3:
                    curFigures[id] = mvFigure;
                    SwitchTurn();
                    break;
                case "Rook":
                    List<cordinates> ListDiagonaleRook = new List<cordinates>();
                    if (curFigures[id].curx > mvFigure.curx && curFigures[id].cury > mvFigure.cury)
                    {
                        for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                        {
                            if (curFigures[id].curx - i != curFigures[id].curx && curFigures[id].cury - i != curFigures[id].cury)
                            {
                                cordinates Diagonale = (curFigures[id].curx - i, curFigures[id].cury - i);
                                ListDiagonaleRook.Add(Diagonale);
                            }
                        }
                    }
                    else if (curFigures[id].curx < mvFigure.curx && curFigures[id].cury > mvFigure.cury)
                    {
                        for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                        {
                            if (curFigures[id].curx + i != curFigures[id].curx && curFigures[id].cury - i != curFigures[id].cury)
                            {
                                cordinates Diagonale = (curFigures[id].curx + i, curFigures[id].cury - i);
                                ListDiagonaleRook.Add(Diagonale);
                            }
                        }
                    }
                    else if (curFigures[id].curx < mvFigure.curx && curFigures[id].cury < mvFigure.cury)
                    {
                        for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                        {
                            if (curFigures[id].curx + i != curFigures[id].curx && curFigures[id].cury + i != curFigures[id].cury)
                            {
                                cordinates Diagonale = (curFigures[id].curx + i, curFigures[id].cury + i);
                                ListDiagonaleRook.Add(Diagonale);
                            }
                        }
                    }
                    else if (curFigures[id].curx > mvFigure.curx && curFigures[id].cury < mvFigure.cury)
                    {
                        for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                        {
                            if (curFigures[id].curx - i != curFigures[id].curx && curFigures[id].cury + i != curFigures[id].cury)
                            {
                                cordinates Diagonale = (curFigures[id].curx - i, curFigures[id].cury + i);
                                ListDiagonaleRook.Add(Diagonale);
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    for (int i = 0; i < ListDiagonaleRook.Count; i++)
                    {
                        for (int j = 0; j < curFigures.Count; j++)
                        {
                            if (ListDiagonaleRook[i].x == curFigures[j].curx && ListDiagonaleRook[i].y == curFigures[j].cury)
                            {
                                if (ListDiagonaleRook[i].x == ListDiagonaleRook[ListDiagonaleRook.Count - 1].x && ListDiagonaleRook[i].y == ListDiagonaleRook[ListDiagonaleRook.Count - 1].y && curFigures[id].side != curFigures[j].side)
                                {
                                    remove(curFigures, j);
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    curFigures[id] = mvFigure;
                    SwitchTurn();
                    break;
                case "King":
                    if (!((curFigures[id].curx + 1 == mvFigure.curx && curFigures[id].cury == mvFigure.cury) || (curFigures[id].curx - 1 == mvFigure.curx && curFigures[id].cury == mvFigure.cury) || (curFigures[id].curx == mvFigure.curx && curFigures[id].cury + 1 == mvFigure.cury) || (curFigures[id].curx == mvFigure.curx && curFigures[id].cury - 1 == mvFigure.cury) || (curFigures[id].curx + 1 == mvFigure.curx && curFigures[id].cury + 1 == mvFigure.cury) || (curFigures[id].curx + 1 == mvFigure.curx && curFigures[id].cury - 1 == mvFigure.cury) || (curFigures[id].curx - 1 == mvFigure.curx && curFigures[id].cury + 1 == mvFigure.cury) || (curFigures[id].curx - 1 == mvFigure.curx && curFigures[id].cury - 1 == mvFigure.cury)))
                    {
                        return;
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].side != mvFigure.side && curFigures[i].cury == mvFigure.cury && curFigures[i].curx == mvFigure.curx)
                        {
                            remove(curFigures, i);
                            goto M5;
                        }
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {
                        if (curFigures[i].curx == mvFigure.curx && curFigures[i].cury == mvFigure.cury)
                        {
                            return;
                        }
                    }
                M5:
                    curFigures[id] = mvFigure;
                    SwitchTurn();
                    break;
                case "Queen":
                    if (!(curFigures[id].curx != mvFigure.curx && curFigures[id].cury == mvFigure.cury) && !(curFigures[id].curx == mvFigure.curx && curFigures[id].cury != mvFigure.cury))
                    {
                        List<cordinates> ListDiagonaleQueen = new List<cordinates>();
                        if (curFigures[id].curx > mvFigure.curx && curFigures[id].cury > mvFigure.cury)
                        {
                            for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                            {
                                if (curFigures[id].curx - i != curFigures[id].curx && curFigures[id].cury - i != curFigures[id].cury)
                                {
                                    cordinates Diagonale = (curFigures[id].curx - i, curFigures[id].cury - i);
                                    ListDiagonaleQueen.Add(Diagonale);
                                }
                            }
                        }
                        else if (curFigures[id].curx < mvFigure.curx && curFigures[id].cury > mvFigure.cury)
                        {
                            for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                            {
                                if (curFigures[id].curx + i != curFigures[id].curx && curFigures[id].cury - i != curFigures[id].cury)
                                {
                                    cordinates Diagonale = (curFigures[id].curx + i, curFigures[id].cury - i);
                                    ListDiagonaleQueen.Add(Diagonale);
                                }
                            }
                        }
                        else if (curFigures[id].curx < mvFigure.curx && curFigures[id].cury < mvFigure.cury)
                        {
                            for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                            {
                                if (curFigures[id].curx + i != curFigures[id].curx && curFigures[id].cury + i != curFigures[id].cury)
                                {
                                    cordinates Diagonale = (curFigures[id].curx + i, curFigures[id].cury + i);
                                    ListDiagonaleQueen.Add(Diagonale);
                                }
                            }
                        }
                        else if (curFigures[id].curx > mvFigure.curx && curFigures[id].cury < mvFigure.cury)
                        {
                            for (int i = 0; i < Math.Abs(mvFigure.curx - curFigures[id].curx) + 1; i++)
                            {
                                if (curFigures[id].curx - i != curFigures[id].curx && curFigures[id].cury + i != curFigures[id].cury)
                                {
                                    cordinates Diagonale = (curFigures[id].curx - i, curFigures[id].cury + i);
                                    ListDiagonaleQueen.Add(Diagonale);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                        for (int i = 0; i < ListDiagonaleQueen.Count; i++)
                        {
                            for (int j = 0; j < curFigures.Count; j++)
                            {
                                if (ListDiagonaleQueen[i].x == curFigures[j].curx && ListDiagonaleQueen[i].y == curFigures[j].cury)
                                {
                                    if (ListDiagonaleQueen[i].x == ListDiagonaleQueen[ListDiagonaleQueen.Count - 1].x && ListDiagonaleQueen[i].y == ListDiagonaleQueen[ListDiagonaleQueen.Count - 1].y && curFigures[id].side != curFigures[j].side)
                                    {
                                        remove(curFigures, j);
                                        goto M6;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        goto M6;
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].side == mvFigure.side && (((curFigures[i].curx == mvFigure.curx && curFigures[id].cury > curFigures[i].cury && curFigures[i].cury >= mvFigure.cury) || (curFigures[i].curx == mvFigure.curx && curFigures[id].cury < curFigures[i].cury && curFigures[i].cury <= mvFigure.cury)) || ((curFigures[i].cury == mvFigure.cury && curFigures[id].curx > curFigures[i].curx && curFigures[i].curx >= mvFigure.curx) || (curFigures[i].cury == mvFigure.cury && curFigures[id].curx < curFigures[i].curx && curFigures[i].curx <= mvFigure.curx))))
                        {
                            return;
                        }
                        if (curFigures[i].side != mvFigure.side && (((curFigures[i].curx == mvFigure.curx && curFigures[id].cury > curFigures[i].cury && curFigures[i].cury > mvFigure.cury) || (curFigures[i].curx == mvFigure.curx && curFigures[id].cury < curFigures[i].cury && curFigures[i].cury < mvFigure.cury)) || ((curFigures[i].cury == mvFigure.cury && curFigures[id].curx > curFigures[i].curx && curFigures[i].curx > mvFigure.curx) || (curFigures[i].cury == mvFigure.cury && curFigures[id].curx < curFigures[i].curx && curFigures[i].curx < mvFigure.curx))))
                        {
                            return;
                        }
                    }
                    for (int i = 0; i < curFigures.Count; i++)
                    {

                        if (curFigures[i].cury == mvFigure.cury && curFigures[i].curx == mvFigure.curx)
                        {
                            remove(curFigures, i);
                        }
                    }
                M6:
                    curFigures[id] = mvFigure;
                    SwitchTurn();
                    break;
            }
            UpdateChess(curFigures);
        }
        //Змiна гравця при кiнцi ходу
        static void SwitchTurn()
        {
            if (turn == "Black")
            {
                turn = "White";
            }
            else if (turn == "White")
            {
                turn = "Black";
            }
        }
        //Функцiя знищення фiгури(якщо фiгура король то кiнець гри)
        static void remove(List<Figure> curFigures, int id)
        {
            Figure mvFigure = curFigures[id];
            mvFigure.curx = id + 20;
            if (mvFigure.side == "White")
            {
                if (mvFigure.type == "King")
                {
                    Console.SetCursorPosition(0, 10);
                    Console.WriteLine("Black wins");
                    GameOver = true;
                }
                mvFigure.cury = 0;
            }
            else if (mvFigure.side == "Black")
            {
                if (mvFigure.type == "King")
                {
                    Console.SetCursorPosition(0, 10);
                    Console.WriteLine("White wins");
                    GameOver = true;
                }
                mvFigure.cury = 1;
            }
            curFigures[id] = mvFigure;
            UpdateChess(curFigures);
        }

        static void Main(string[] args)
        {
            List<Figure> curFigures = FiguresSpawn();
            Console.SetCursorPosition(1, 1);
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    if (GameOver)
                    {
                        return;
                    }
                    else
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        KeyboardOperation(key.Key, curFigures);
                    }
                }
            }
        }
    }
}