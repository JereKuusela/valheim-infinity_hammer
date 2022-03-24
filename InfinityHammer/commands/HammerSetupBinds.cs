namespace InfinityHammer {
  public class HammerSetupBindsCommand {
    private static void BindGeneral(Terminal terminal) {
      terminal.TryRunCommand("unbind keypad0");
      terminal.TryRunCommand("bind keypad0 hammer");
      terminal.TryRunCommand("unbind keypad1");
      terminal.TryRunCommand("bind keypad1 hammer_scale_down");
      terminal.TryRunCommand("unbind keypad2");
      terminal.TryRunCommand("bind keypad2 hammer_scale");
      terminal.TryRunCommand("unbind keypad3");
      terminal.TryRunCommand("bind keypad3 hammer_scale_up");
      terminal.TryRunCommand("unbind keypad7");
      terminal.TryRunCommand("bind keypad7 hammer_undo");
      terminal.TryRunCommand("unbind keypad8");
      terminal.TryRunCommand("bind keypad8 hammer_config enabled");
      terminal.TryRunCommand("unbind keypad9");
      terminal.TryRunCommand("bind keypad9 hammer_redo");
    }
    public HammerSetupBindsCommand() {
      CommandWrapper.RegisterEmpty("hammer_setup_binds");
      new Terminal.ConsoleCommand("hammer_setup_binds", "Sets recommended key bindings.", delegate (Terminal.ConsoleEventArgs args) {
        BindGeneral(args.Context);
        var isDev = InfinityHammer.IsServerDevcommands;
        var modifier = (isDev ? " keys=-leftalt" : "");
        args.Context.TryRunCommand("unbind rightcontrol");
        args.Context.TryRunCommand("bind rightcontrol hammer_offset 0,0,0");
        args.Context.TryRunCommand("unbind rightarrow");
        args.Context.TryRunCommand("bind rightarrow hammer_move_z -0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind rightarrow hammer_move_z -1 keys=leftalt");
        args.Context.TryRunCommand("unbind leftarrow");
        args.Context.TryRunCommand("bind leftarrow hammer_move_z 0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind leftarrow hammer_move_z 1 keys=leftalt");
        args.Context.TryRunCommand("unbind downarrow");
        args.Context.TryRunCommand("bind downarrow hammer_move_y -0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind downarrow hammer_move_y -1 keys=leftalt");
        args.Context.TryRunCommand("unbind uparrow");
        args.Context.TryRunCommand("bind uparrow hammer_move_y 0.1" + modifier);
        if (isDev) args.Context.TryRunCommand("bind uparrow hammer_move_y 1 keys=leftalt");
        Helper.AddMessage(args.Context, "Keybindings set for Infinity Hammer (with Dedicated Server Devcommands mod).");
      });
    }
  }
}
