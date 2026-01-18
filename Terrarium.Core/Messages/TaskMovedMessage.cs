namespace Terrarium.Core.Messages;

public record TaskMovedMessage(string TaskId, string NewColumnId, string OldColumnId, string NewColumnTitle);