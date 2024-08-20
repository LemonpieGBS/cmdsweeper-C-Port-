using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cmdsweeper__C__Port_
{
    internal class MainProgram
    {
        // =========================================
        // Coloring Class
        class COLOR_FF
        {
            public ConsoleColor fg_color;
            public ConsoleColor? bg_color = null;

            public COLOR_FF(ConsoleColor fg_color, ConsoleColor bg_color) {
                this.fg_color = fg_color; this.bg_color = bg_color; }

            public COLOR_FF(ConsoleColor fg_color) {
                this.fg_color = fg_color; }

            public void Apply() {
                Console.ForegroundColor = this.fg_color;
                Console.BackgroundColor = this.bg_color ?? Console.BackgroundColor; }

            public static void Reset() { Console.ResetColor(); }
        }

        // =========================================
        // Color Constants

        static readonly COLOR_FF BRIGHT_RED = new COLOR_FF(ConsoleColor.White, ConsoleColor.Red);
        static readonly COLOR_FF BRIGHT_GREEN = new COLOR_FF(ConsoleColor.White, ConsoleColor.Green);
        static readonly COLOR_FF INV_BRIGHT_RED = new COLOR_FF(ConsoleColor.Red, ConsoleColor.White);

        static readonly COLOR_FF RED = new COLOR_FF(ConsoleColor.Red);
        static readonly COLOR_FF CYAN = new COLOR_FF(ConsoleColor.Cyan);
        static readonly COLOR_FF YELLOW = new COLOR_FF(ConsoleColor.Yellow);
        static readonly COLOR_FF GRAY = new COLOR_FF(ConsoleColor.Gray);

        // =========================================
        // Other declarations
        enum INPUTS { LEFT = 1, RIGHT = 2, UP = 3, DOWN = 4, ENTER = 6, B = 7, F = 8, DNULL = 0 };
        static COLOR_FF[] txtcolors ={ new COLOR_FF(ConsoleColor.White),
                                new COLOR_FF(ConsoleColor.Blue),
                                new COLOR_FF(ConsoleColor.Green), 
                                new COLOR_FF(ConsoleColor.Red),
                                new COLOR_FF(ConsoleColor.DarkBlue), 
                                new COLOR_FF(ConsoleColor.DarkMagenta), 
                                new COLOR_FF(ConsoleColor.Cyan), 
                                new COLOR_FF(ConsoleColor.Magenta),
                                new COLOR_FF(ConsoleColor.Gray) };
        static int bombs_flagged = 0, correct_mines = 0;

        // =========================================
        // Game Classes
        class FIELD {
            public int bomb_amount;
            public bool bomb_innit = false, field_shown = false,
                field_flagged = false;
        }

        class GM_TEMPLATE {
            public string name = "bs_template";
            public int gm_width = 10, gm_height = 10,
                gm_bomb_amount = 10;
            public COLOR_FF color_identifier = new COLOR_FF(ConsoleColor.White);

            public GM_TEMPLATE(string name, int gm_width, int gm_height, int gm_bomb_amount, COLOR_FF color_identifier) {
                this.name = name; this.gm_width = gm_width; this.gm_height = gm_height;
                this.gm_bomb_amount = gm_bomb_amount; this.color_identifier = color_identifier;
            }
        }

        // =========================================
        // Function Definitions
        static int get_integer()
        {
            bool succesful_input;

            do
            {
                succesful_input = true;
                try
                {
                    string input = Console.ReadLine() ?? string.Empty;
                    int input_parsed = Convert.ToInt32(input);
                    return input_parsed;

                }
                catch (FormatException)
                {
                    Console.WriteLine("El dato ingresado no se pudo convertir a numero! " +
                        "Ingrese de nuevo:\n> ");
                    succesful_input = false;
                }

            } while (!succesful_input);
            return 0;
        }

        static float clamp(float val, float low_boundary, float high_boundary)
        {
            if (val > high_boundary) val = high_boundary;
            else if (val < low_boundary) val = low_boundary;

            return val;
        }

        static void array_randomize(int[] array)
        {
            int shufflecontainer;
            Random randomizer = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < array.GetLength(0); i++)
            {
                int j = randomizer.Next(0, array.GetLength(0));
                shufflecontainer = array[j];

                array[j] = array[i];
                array[i] = shufflecontainer;
            }
        }

        static void analyze_bombs(FIELD[] field_arr, int field_position, int brd_width, int brd_height)
        {

            // Detect the limits
            bool alt_right = false, alt_left = false, alt_up = false, alt_down = false;
            if (field_position % brd_width == 0) alt_left = true;
            if ((field_position + 1) % brd_width == 0) alt_right = true;
            if (field_position < brd_width) alt_up = true;
            if (field_position >= brd_width * brd_height - brd_width) alt_down = true;

            int bomb_amounts = 0;

            if (!alt_right)
            {
                bomb_amounts += 
                    field_arr[field_position + 1].bomb_innit ? 1 : 0;

                if (!alt_down) { 
                    bomb_amounts += 
                        field_arr[field_position + brd_width + 1].bomb_innit ? 1 : 0; }
                
                if (!alt_up) { 
                    bomb_amounts += 
                        field_arr[field_position - brd_width + 1].bomb_innit ? 1 : 0; }
            }

            if (!alt_left)
            {
                bomb_amounts += 
                    field_arr[field_position - 1].bomb_innit ? 1 : 0;

                if (!alt_down) { 
                    bomb_amounts += 
                        field_arr[field_position + brd_width - 1].bomb_innit ? 1 : 0; }
                
                if (!alt_up) { 
                    bomb_amounts += 
                        field_arr[field_position - brd_width - 1].bomb_innit ? 1 : 0; }
            }

            if (!alt_down) { 
                bomb_amounts += 
                    field_arr[field_position + brd_width].bomb_innit ? 1 : 0; }

            if (!alt_up) { 
                bomb_amounts += 
                    field_arr[field_position - brd_width].bomb_innit ? 1 : 0; }

            field_arr[field_position].bomb_amount = bomb_amounts;
        }

        static int analyze_flags(FIELD[] field_arr, int field_position, int brd_width, int brd_height)
        {

            // Detect the limits
            bool alt_right = false, alt_left = false, alt_up = false, alt_down = false;
            if (field_position % brd_width == 0) alt_left = true;
            if ((field_position + 1) % brd_width == 0) alt_right = true;
            if (field_position < brd_width) alt_up = true;
            if (field_position >= brd_width * brd_height - brd_width) alt_down = true;

            int flag_amount = 0;

            if (!alt_right)
            {
                flag_amount += 
                    field_arr[field_position + 1].field_flagged ? 1 : 0;

                if (!alt_down) { flag_amount += field_arr[field_position + brd_width + 1].field_flagged ? 1 : 0; }
                if (!alt_up) { flag_amount += field_arr[field_position - brd_width + 1].field_flagged ? 1 : 0; }
            }

            if (!alt_left)
            {
                flag_amount += field_arr[field_position - 1].field_flagged ? 1 : 0;
                if (!alt_down) { flag_amount += field_arr[field_position + brd_width - 1].field_flagged ? 1 : 0; }
                if (!alt_up) { flag_amount += field_arr[field_position - brd_width - 1].field_flagged ? 1 : 0; }
            }

            if (!alt_down) { flag_amount += field_arr[field_position + brd_width].field_flagged ? 1 : 0; }
            if (!alt_up) { flag_amount += field_arr[field_position - brd_width].field_flagged ? 1 : 0; }

            return flag_amount;
        }

        static void gotoxy(int x, int y) {
            Console.SetCursorPosition(x, y);
        }


        static void update_cursor(int pos, int old_pos, int brd_width, COLOR_FF color)
        {
            int x_coord = pos % brd_width;
            int y_coord = (int) (Math.Ceiling((float)(pos + 1) / brd_width) - 1);

            int old_x_coord = old_pos % brd_width;
            int old_y_coord = (int) (Math.Ceiling((float)(old_pos + 1) / brd_width) - 1);

            color.Apply();
            gotoxy(x_coord * 3, y_coord);
            Console.Write("[");
            gotoxy(x_coord * 3 + 2, y_coord);
            Console.Write("]");

            COLOR_FF.Reset();
            if (pos != old_pos)
            {
                gotoxy(old_x_coord * 3, old_y_coord);
                Console.Write("[");
                gotoxy(old_x_coord * 3 + 2, old_y_coord);
                Console.Write("]");
            }
        }

        static void show_field(FIELD[] field_arr, int pos, int brd_width, int brd_height)
        {

            if (field_arr[pos].field_shown) return;

            int x_coord = pos % brd_width;
            int y_coord = (int) (Math.Ceiling((float)(pos + 1) / brd_width) - 1);
            field_arr[pos].field_shown = true;

            if (field_arr[pos].bomb_innit)
            {

                // Reveal a mine :(
                BRIGHT_RED.Apply();
                gotoxy(x_coord * 3, y_coord);
                Console.Write("[X]");

            }
            else if (field_arr[pos].bomb_amount == 0)
            {

                // Reveal all surrounding area if space is 0
                gotoxy(x_coord * 3 + 1, y_coord);
                Console.Write("-");
                show_surrounding_fields(field_arr, pos, brd_width, brd_height);
                correct_mines++;

            }
            else
            {

                // Reveal the numbah
                txtcolors[field_arr[pos].bomb_amount].Apply();
                gotoxy(x_coord * 3 + 1, y_coord);
                Console.Write(field_arr[pos].bomb_amount);
                correct_mines++;
            }

            if (field_arr[pos].field_flagged) { field_arr[pos].field_flagged = false; bombs_flagged--; }

            COLOR_FF.Reset();
        }

        static void show_surrounding_fields(FIELD[] field_arr, int pos, int brd_width, int brd_height)
        {

            int x_coord = pos % brd_width;
            int y_coord = Convert.ToInt32(Math.Ceiling((float)(pos + 1) / brd_width) - 1);

            bool alt_left = (x_coord == 0),
                alt_right = (x_coord == brd_width - 1),
                alt_up = (y_coord == 0),
                alt_down = (y_coord == brd_height - 1);

            if (!alt_right)
            {
                show_field(field_arr, pos + 1, brd_width, brd_height);

                if (!alt_down) { show_field(field_arr, pos + brd_width + 1, brd_width, brd_height); }
                if (!alt_up) { show_field(field_arr, pos - brd_width + 1, brd_width, brd_height); }
            }

            if (!alt_left)
            {
                show_field(field_arr, pos - 1, brd_width, brd_height);
                if (!alt_down) { show_field(field_arr, pos + brd_width - 1, brd_width, brd_height); }
                if (!alt_up) { show_field(field_arr, pos - brd_width - 1, brd_width, brd_height); }
            }

            if (!alt_down) { show_field(field_arr, pos + brd_width, brd_width, brd_height); }
            if (!alt_up) { show_field(field_arr, pos - brd_width, brd_width, brd_height); }
        }

        static void flag_field(FIELD[] field_arr, int pos, int brd_width)
        {

            if (field_arr[pos].field_shown) { return; }

            int x_coord = pos % brd_width;
            int y_coord = (int) Math.Ceiling((float)(pos + 1) / brd_width) - 1;

            field_arr[pos].field_flagged = !field_arr[pos].field_flagged;
            if (field_arr[pos].field_flagged)
            {
                BRIGHT_RED.Apply();
                gotoxy(x_coord * 3 + 1, y_coord);
                Console.Write("B");
                bombs_flagged++;
            }
            else
            {
                COLOR_FF.Reset();
                gotoxy(x_coord * 3 + 1, y_coord);
                Console.Write(" ");
                bombs_flagged--;
            }
        }

        static void screen_refresh(FIELD[] field_arr, int width, int height)
        {
            gotoxy(0, 0);
            COLOR_FF.Reset();
            Console.Clear();

            int ttl_field_roll = 0, x_coord, y_coord;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {

                    x_coord = ttl_field_roll % width;
                    y_coord = Convert.ToInt32(Math.Ceiling((float)(ttl_field_roll + 1) / width) - 1);

                    gotoxy(x_coord * 3, i);
                    Console.Write("[ ]");

                    if (!field_arr[ttl_field_roll].field_shown)
                    {

                        if (field_arr[ttl_field_roll].field_flagged)
                        {
                            BRIGHT_RED.Apply();
                            gotoxy(x_coord * 3 + 1, y_coord);
                            Console.Write("B");
                        }
                    }
                    else
                    {

                        gotoxy(x_coord * 3 + 1, y_coord);
                        if (field_arr[ttl_field_roll].bomb_amount != 0)
                        {
                            txtcolors[field_arr[ttl_field_roll].bomb_amount].Apply();
                            Console.Write(field_arr[ttl_field_roll].bomb_amount);
                        }
                        else Console.Write("-");
                    }

                    ttl_field_roll++;
                    COLOR_FF.Reset();
                }
                Console.Write("\n");
            }
        }

        static INPUTS detect_input()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey input_key = Console.ReadKey(true).Key;
                switch (input_key)
                {
                    case (ConsoleKey.A): { return INPUTS.LEFT; }
                    case (ConsoleKey.D): { return INPUTS.RIGHT; }
                    case (ConsoleKey.W): { return INPUTS.UP; }
                    case (ConsoleKey.S): { return INPUTS.DOWN; }
                    case (ConsoleKey.Enter): { return INPUTS.ENTER; }
                    case (ConsoleKey.B): { return INPUTS.B; }
                    case (ConsoleKey.F): { return INPUTS.F; }
                    default: { return INPUTS.DNULL; }
                }
            }
            else return INPUTS.DNULL;
        }

        static void Main()
        {
            // Randomize the seed and declare stuff yknow
            Console.Clear();
            COLOR_FF.Reset();

            const int sleep_time = 10, hold_module = 7;
            float seconds = 0;
            int width = 0, height = 0, bomb_amount = 0, game_cursor = 0, old_game_cursor = 1,
                current_frame = 0, return_time = 2, b_time = 0, f_time = 0;

            COLOR_FF cursor_color = BRIGHT_GREEN;
            int user_input, template_chosen = -1, seconds_length;
            bool game_loop = true, cancel_movement, win = false;

            INPUTS move_receiver = INPUTS.DNULL, old_move_receiver;

            string mode_string_1, mode_string_2, timer_string, version = "1.0.0";

            // Set Game Templates
            const int template_amount = 5;
            GM_TEMPLATE[] templates = {
                new GM_TEMPLATE("Easy", 10, 10, 10, new COLOR_FF(ConsoleColor.Blue)),
                new GM_TEMPLATE("Medium", 16, 16, 40, new COLOR_FF(ConsoleColor.Green)),
                new GM_TEMPLATE("Hard", 30, 16, 99, new COLOR_FF(ConsoleColor.Yellow)),
                new GM_TEMPLATE("Expert", 36, 20, 144, new COLOR_FF(ConsoleColor.Red)),
                new GM_TEMPLATE("Master", 42, 24, 202, BRIGHT_RED)
            };

            // Declare pointers for dynamic arrays
            FIELD[] fields;
            int[] bomb_pos;

            // Starting game title
            RED.Apply();

            // Site for the ASCII art: https://patorjk.com/software/taag/#p=display&f=Doom&t=cmdsweeper
            Console.Write("                    _                                       \n");
            Console.Write("                   | |                                      \n");
            Console.Write("  ___ _ __ ___   __| |_____      _____  ___ _ __   ___ _ __ \n");
            Console.Write(" / __| '_ ` _ \\ / _` / __\\ \\ /\\ / / _ \\/ _ \\ '_ \\ / _ \\ '__|\n");
            Console.Write("| (__| | | | | | (_| \\__ \\  V  V /  __/  __/ |_) |  __/ |   \n");
            Console.Write(" \\___|_| |_| |_|\\__,_|___/ \\_/\\_/ \\___|\\___| .__/ \\___|_|   \n");
            Console.Write("                                           | |              \n");
            Console.Write("                                           |_|              \n");

            YELLOW.Apply();
            Console.Write("\n(C# Port). ");
            CYAN.Apply();
            Console.Write($"V. {version}, by @LemonpieGBS\n\n");
            COLOR_FF.Reset();

            // Select a custom size or template
            Console.Write("Would you like to select a template for the game or set the size yourself?");
            Console.Write("\n #1. Use templates (Easy / Hard / etc.)");
            Console.Write("\n #2. Play a Custom Game\n");

            CYAN.Apply();
            Console.Write("\nEnter your preferred option: ");
            COLOR_FF.Reset();
            user_input = get_integer();

            // Cin Failsafe
            //if(!std::cin) { std::cin.clear(); std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n'); user_input = -1; }

            while (user_input != 1 && user_input != 2)
            {
                CYAN.Apply();
                Console.Write("\nPlease enter a valid option: ");
                COLOR_FF.Reset();
                user_input = get_integer();

            }
            Console.Clear();

            if (user_input == 1)
            {

                CYAN.Apply();
                Console.Write("Available templates: \n\n");
                COLOR_FF.Reset();

                // Display all the templates
                for (int i = 0; i < template_amount; i++)
                {
                    Console.Write($" #{i + 1}. ");
                    templates[i].color_identifier.Apply();
                    Console.Write(templates[i].name);
                    COLOR_FF.Reset();
                    Console.Write($" ({templates[i].gm_width}x{templates[i].gm_height}), {templates[i].gm_bomb_amount} mines)\n");
                }
                RED.Apply();
                Console.Write("\n# WARNING: Some game sizes may not be available for your display size, in case graphic glitches arise, make sure to zoom out the console and press F to refresh the display\n");
                CYAN.Apply();
                Console.Write("# TIP: You can zoom out the console with CNTRL + Scroll Wheel\n\n");

                COLOR_FF.Reset();

                Console.Write("\nSelect a template: ");
                user_input = get_integer();

                // While not a valid option, the user must input a valid number
                while (!(user_input >= 1 && user_input <= template_amount))
                {

                    Console.Write("\nPlease enter a valid option: ");
                    user_input = get_integer();

                }

                // Apply template
                width = templates[user_input - 1].gm_width;
                height = templates[user_input - 1].gm_height;
                bomb_amount = templates[user_input - 1].gm_bomb_amount;
                template_chosen = user_input - 1;

            }
            else if (user_input == 2)
            {

                CYAN.Apply();
                Console.Write("Custom game setup: \n");

                // Let's set the dimensions for the game
                YELLOW.Apply();
                Console.Write("\n Specify the Width of the game: ");
                COLOR_FF.Reset();
                width = get_integer();

                YELLOW.Apply();
                Console.Write("\n Specify the Height of the game: ");
                COLOR_FF.Reset();
                height = get_integer();

                YELLOW.Apply();
                Console.Write("\n Specify the Number of Bombs: ");
                COLOR_FF.Reset();
                bomb_amount = get_integer();

                template_chosen = -1;

                RED.Apply();
                Console.Write("\n# WARNING: Some game sizes may not be available for your display size, in case graphic glitches arise, make sure to zoom out the console and press F to refresh the display\n");
                CYAN.Apply();
                Console.Write("# TIP: You can zoom out the console with CNTRL + Scroll Wheel\n\n");

                COLOR_FF.Reset();

                Console.ReadKey(true);

            }

            COLOR_FF.Reset();
            // Let's clamp width and height just in case
            width = (width < 10) ? 10 : width;
            height = (height < 10) ? 10 : height;

            // Let's declare the total area & clamp the bomb amount
            int ttl_area = width * height;
            bomb_amount = (int) clamp(bomb_amount, 1.0f, width * height - 1);

            fields = new FIELD[ttl_area];
            for (int i = 0; i < fields.GetLength(0); i++) fields[i] = new FIELD();

            bomb_pos = new int[ttl_area];

            // Let's set up the bomb randomizing process
            for (int i = 0; i < ttl_area; i++) { bomb_pos[i] = i; }
            array_randomize(bomb_pos);

            // Now we just give the selected fields some bombs
            for (int i = 0; i < bomb_amount; i++) { fields[bomb_pos[i]].bomb_innit = true; }

            // And we now calculate each field's bomb amount
            for (int i = 0; i < ttl_area; i++) { analyze_bombs(fields, i, width, height); }

            // Clear the Screen and draw the game!
            Console.Clear();
            int ttl_field_roll = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Console.Write("[ ]");
                    ttl_field_roll++;
                }
                Console.Write("\n");
            }

            // Declare the Mode Text
            mode_string_1 = (template_chosen == -1) ? "Custom" : templates[template_chosen].name;
            mode_string_2 = $" ({width}x{height})";

            mode_string_1 = "[" + mode_string_1 + "]" + mode_string_2;

            // Update the cursor and start the game loop
            update_cursor(game_cursor, old_game_cursor, width, cursor_color);

            // Start the timer
            DateTime start = DateTime.UtcNow;

            while (game_loop)
            {

                seconds = (seconds < 9999) ? (int) (DateTime.UtcNow - start).TotalSeconds : 9999;

                // Detect inputs from the move receiver
                old_move_receiver = move_receiver;

                INPUTS current_input = detect_input();
                move_receiver = (current_input > INPUTS.DOWN) ? INPUTS.DNULL : current_input;

                // Detect if a hold is happening
                if (old_move_receiver == move_receiver && move_receiver != INPUTS.DNULL)
                {
                    // Update the current frame and reset it when it gets past the hold module (cycle)
                    current_frame = (current_frame >= hold_module) ? 0 : current_frame + 1;
                    cancel_movement = (current_frame != 0);
                }
                else { current_frame = -hold_module; cancel_movement = false; }

                if (!cancel_movement && (move_receiver != INPUTS.DNULL || old_move_receiver != INPUTS.DNULL))
                {

                    // Assign old_game_cursor to game_cursor before moving
                    old_game_cursor = game_cursor;

                    // Move the game_cursor based on the input
                    switch (move_receiver)
                    {
                        case (INPUTS.LEFT): game_cursor = (game_cursor % width == 0) ? game_cursor : game_cursor - 1; break;
                        case (INPUTS.RIGHT): game_cursor = ((game_cursor + 1) % width == 0) ? game_cursor : game_cursor + 1; break;
                        case (INPUTS.UP): game_cursor = (game_cursor < width) ? game_cursor : game_cursor - width; break;
                        case (INPUTS.DOWN): game_cursor = (game_cursor >= ttl_area - width) ? game_cursor : game_cursor + width; break;
                        default: break;
                    }

                }
                cursor_color = BRIGHT_GREEN;

                // Detect stuff happening when enter is pressed
                if (current_input != INPUTS.DNULL) {

                    if (current_input == INPUTS.ENTER)
                    {
                        return_time++;
                        cursor_color = BRIGHT_RED;
                    }
                    else { return_time = 0; }

                    if (current_input == INPUTS.B)
                    {
                        b_time++;
                        cursor_color = BRIGHT_RED;
                    }
                    else { b_time = 0; }

                    if (current_input == INPUTS.F)
                    {
                        f_time++;
                    }
                    else { f_time = 0; }
                } else
                {
                    return_time = 0;
                    b_time = 0;
                    f_time = 0;
                }

                // Update cursor
                update_cursor(game_cursor, old_game_cursor, width, cursor_color);

                // If enter is pressed for the first frame, it will mine
                if (return_time == 1)
                {

                    if (!fields[game_cursor].field_flagged) show_field(fields, game_cursor, width, height);

                    if (fields[game_cursor].bomb_innit && !fields[game_cursor].field_flagged)
                    {
                        for (int i = 0; i < ttl_area; i++)
                        {
                            if (fields[i].bomb_innit)
                            {
                                int x_coord = i % width;
                                int y_coord = (int) Math.Ceiling((float)(i + 1) / width) - 1;

                                // Reveal a mine :(
                                BRIGHT_RED.Apply();
                                gotoxy(x_coord * 3, y_coord);
                                Console.Write("[X]");
                            }
                            else if (!fields[i].bomb_innit && fields[i].field_flagged)
                            {
                                int x_coord = i % width;
                                int y_coord = (int) Math.Ceiling((float)(i + 1) / width) - 1;

                                // Reveal a mine :(
                                INV_BRIGHT_RED.Apply();
                                gotoxy(x_coord * 3, y_coord);
                                Console.Write("[B]");
                            }
                        }
                        game_loop = false;
                    }

                }

                // If B is pressed for the first frame, it will flag
                if (b_time == 1) { flag_field(fields, game_cursor, width); }

                // If F is pressed refresh the screen
                if (f_time == 1 && game_loop) { screen_refresh(fields, width, height); }

                // Check win condition
                if (correct_mines == ttl_area - bomb_amount)
                {
                    game_loop = false;
                    win = true;
                    bombs_flagged = bomb_amount;

                    COLOR_FF.Reset();
                    int x_coord = game_cursor % width;
                    int y_coord = (int) Math.Ceiling((float)(game_cursor + 1) / width) - 1;

                    gotoxy(x_coord * 3, y_coord);
                    Console.Write("[");
                    gotoxy(x_coord * 3 + 2, y_coord);
                    Console.Write("]");
                }

                // Let's display the text below
                COLOR_FF.Reset();

                // Mine Amount Text [Centered Left, 1st Line]
                gotoxy(0, height + 1);
                Console.Write($"{bombs_flagged}/{bomb_amount} mines");

                // Time Text [Center Right, 1st Line]
                seconds_length = 4 - seconds.ToString().Length;
                timer_string = "Time: ";
                for (int i = 1; i <= seconds_length; i++) { timer_string += "0"; }
                timer_string += seconds.ToString();

                gotoxy(width * 3 - timer_string.Length, height + 1);
                Console.Write(timer_string);

                // Difficulty Text [Centered, 2nd Line]
                if (template_chosen != -1) templates[template_chosen].color_identifier.Apply();
                else if (width + height > 64 && (float)bomb_amount / (float)ttl_area >= 0.2) { BRIGHT_RED.Apply(); } else { GRAY.Apply(); }
                gotoxy((width * 3) / 2 - mode_string_1.Length / 2, height + 2);
                Console.Write(mode_string_1);

                // Let the program sleep between frames
                Thread.Sleep(sleep_time);
            }

            // Display final message
            COLOR_FF.Reset();

            // Set everything back to normal yknow
            if (win)
            {

                YELLOW.Apply();
                string game_won = "YOU HAVE WON THE MINESWEEPER!";
                gotoxy((width * 3) / 2 - game_won.Length / 2, height + 4);
                Console.Write(game_won);

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < ttl_area; j++)
                    {
                        if (!fields[j].bomb_innit) continue;

                        int x_coord = j % width;
                        int y_coord = (int) Math.Ceiling((float)(j + 1) / width) - 1;

                        // Reveal a mine :(
                        if (i % 2 == 0) {
                            INV_BRIGHT_RED.Apply(); } else { BRIGHT_RED.Apply(); }
                        gotoxy(x_coord * 3, y_coord);
                        Console.Write("[!]");
                    }
                    gotoxy(0, height + 4);
                    Thread.Sleep(500);
                }

                COLOR_FF.Reset();
            }
            else
            {
                RED.Apply();
                string game_lost = "Better luck next time! :(";
                gotoxy((width * 3) / 2 - game_lost.Length / 2, height + 4);
                Console.Write(game_lost);
                COLOR_FF.Reset();
            }

            gotoxy(0, height + 6);
            Thread.Sleep(2000);
            Console.ReadKey(true);
        }
    }
}
