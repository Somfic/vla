using Vla.Addon;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests;

public class EnumExtensionsTests
{
	[Test]
	public void GetValueName_EnumValueWithAttribute_ReturnsAttributeValue()
	{
		Assert.Multiple(() =>
		{
			Assert.That(EnumWithAttribute.Test.GetValueName(), Is.EqualTo("Value A"));
			Assert.That(EnumWithAttribute.Test2.GetValueName(), Is.EqualTo("Value B"));
		});
	}

	[Test]
	public void GetValueName_EnumValueWithoutAttribute_ReturnsValueName()
	{
		Assert.Multiple(() =>
		{
			Assert.That(EnumWithoutAttribute.Test.GetValueName(), Is.EqualTo("Test"));
			Assert.That(EnumWithoutAttribute.Test2.GetValueName(), Is.EqualTo("Test2"));
		});
	}

	[Test]
	public void GetValueNameFromEnum_EnumValueWithAttribute_ReturnsAttributeValue()
	{
		Assert.Multiple(() =>
		{
			Assert.That(EnumExtensions.GetValueNameFromEnum(typeof(EnumWithAttribute), "Test"), Is.EqualTo("Value A"));
			Assert.That(EnumExtensions.GetValueNameFromEnum(typeof(EnumWithAttribute), "Test2"), Is.EqualTo("Value B"));
		});
	}

	[Test]
	public void GetValueNameFromEnum_EnumValueWithoutAttribute_ReturnsValueName()
	{
		Assert.Multiple(() =>
		{
			Assert.That(EnumExtensions.GetValueNameFromEnum(typeof(EnumWithoutAttribute), "Test"), Is.EqualTo("Test"));
			Assert.That(EnumExtensions.GetValueNameFromEnum(typeof(EnumWithoutAttribute), "Test2"),
				Is.EqualTo("Test2"));
		});
	}

	private enum EnumWithAttribute
	{
		[NodeEnumValue("Value A")]
		Test,

		[NodeEnumValue("Value B")]
		Test2
	}

	private enum EnumWithoutAttribute
	{
		Test,
		Test2
	}
}