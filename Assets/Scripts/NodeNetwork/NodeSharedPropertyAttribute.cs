using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSharedPropertyAttribute : Attribute
{
	public string OnChange { get; }

	public NodeSharedPropertyAttribute(string onChange)
	{
		OnChange = onChange;
	}

	public NodeSharedPropertyAttribute()
	{

	}
}
