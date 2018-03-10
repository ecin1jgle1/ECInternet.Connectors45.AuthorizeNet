using System;
using Realisable.JobDefinition;
using Realisable.Resources;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	/// <summary>
	/// Provides a helper class for creating TransformDefinition, TransformRecordDefinition and TransformRecordField objects
	/// required to create to create a valid TransformDefinition object.
	/// </summary>
	public class TransformDefinitionBuilder
	{
		public static void CreateField(TransformRecordDefinition recDef, string name)
		{
			TransformRecordField fld = recDef.CreateField(name);
			fld.Type = IntManFieldTypeEnum.eString;
		}

		public static void CreateField(TransformRecordDefinition recDef, string name, IntManFieldTypeEnum type)
		{
			TransformRecordField fld = recDef.CreateField(name);
			fld.Type = type;
		}

		public static void CreateField(TransformRecordDefinition recDef, string name, string[] attribs, string[] attribVals)
		{
			TransformRecordField fld = recDef.CreateField(name);
			fld.Type = IntManFieldTypeEnum.eString;

			if (attribs.Length != attribVals.Length)
			{
				throw new ArgumentException("Error creating fields; attribs and attribVals lengths do not match.");
			}

			for (int i = 0; i < attribs.Length; i++)
			{
				fld.Attributes.SetAttribute(attribs[i], attribVals[i]);
			}
		}

		public static void CreateField(TransformRecordDefinition recDef, string name, IntManFieldTypeEnum type, string[] attribs, string[] attribVals)
		{
			TransformRecordField fld = recDef.CreateField(name);
			fld.Type = type;

			if (attribs.Length != attribVals.Length)
			{
				throw new ArgumentException("Error creating fields; attribs and attribVals lengths do not match.");
			}

			for (int i = 0; i < attribs.Length; i++)
			{
				fld.Attributes.SetAttribute(attribs[i], attribVals[i]);
			}
		}

		public static TransformRecordDefinition CreateRecord(TransformDefinition tfmDef, string recordId, string parentId)
		{
			TransformRecordDefinition result = tfmDef.CreateTransformRecord(recordId, parentId);
			result.ParentRecordId = parentId;
			return result;
		}
	}
}
