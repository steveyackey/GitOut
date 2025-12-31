// This file is kept for backward compatibility
// The interface has been moved to GitOut.Domain.Interfaces

using GitOut.Domain.Interfaces;

namespace GitOut.Application.Interfaces;

// Re-export the interface for backward compatibility
public interface IGitCommandExecutor : Domain.Interfaces.IGitCommandExecutor
{
}
