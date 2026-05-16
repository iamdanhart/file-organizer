using System.Collections.Generic;

namespace file_organizer;

public record AppConfig                                                                                                                                          
{                                                                                                                                                               
    public Dictionary<string, string[]> Categories { get; set; } = new();                                                                                       
}  