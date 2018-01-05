﻿using System.Collections.Generic;
using System.IO.Abstractions;
using VsDiffDuplicateHandler.Models;

namespace VsDiffDuplicateHandler.Services
{
    public interface IDuplicateReader : IEnumerable<DuplicateGroup>
    {
        bool CanHandle(FileInfoBase fileInfo);
    }
}