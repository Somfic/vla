using Vla.Helpers;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Types;

public class Extensions
{
	[Test]
	public void GetDefaultValueForType_String_ReturnsEmptyString()
	{
		Assert.That(typeof(string).GetDefaultValueForType(), Is.EqualTo(string.Empty));
	}
	
	[Test]
	public void GetDefaultValueForType_Int_ReturnsZero()
	{
		Assert.That(typeof(int).GetDefaultValueForType(), Is.EqualTo(0));
	}
	
	[Test]
	public void GetDefaultValueForType_NullableInt_ReturnsNull()
	{
		Assert.That(typeof(int?).GetDefaultValueForType(), Is.Null);
	}
	
	[Test]
	public void GetDefaultValueForType_Boolean_ReturnsFalse()
	{
		Assert.That(typeof(bool).GetDefaultValueForType(), Is.False);
	}

	[Test]
	public void GetDefaultValueForType_Struct_ReturnsDefaultStruct()
	{
		Assert.That(typeof(DateTime).GetDefaultValueForType(), Is.EqualTo(default(DateTime)));
	}
	
	[Test]
	public void GetDefaultValueForType_Class_ReturnsNull()
	{
		Assert.That(typeof(object).GetDefaultValueForType(), Is.Null);
	}

	[Test]
	public void GetDefaultValueForType_Enum_ReturnsDefaultEnum()
	{
		Assert.That(typeof(FileMode).GetDefaultValueForType(), Is.EqualTo(default(FileMode)));
	}
}