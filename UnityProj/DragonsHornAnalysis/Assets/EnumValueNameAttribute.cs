using System;

public class EnumValueNameAttribute : Attribute
{
	public EnumValueNameAttribute(params string[] names)
	{
		Names = names;
	}

	public string[] Names { get; }
}
