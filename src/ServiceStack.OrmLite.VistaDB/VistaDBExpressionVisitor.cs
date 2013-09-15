using System;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
	public class VistaDBExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
	    public override string ToSelectStatement()
        {
            if (!Skip.HasValue && !Rows.HasValue)
                return base.ToSelectStatement();

            AssertValidSkipRowValues();

            var skip = this.Skip.GetValueOrDefault();
            var take = this.Rows.HasValue ? this.Rows.Value : int.MaxValue;

	        var sql = "";
            if (skip == 0)
            {
                sql = base.ToSelectStatement();
                if (take == int.MaxValue)
                    return sql; ;

                if (sql == null || sql.Length < "SELECT".Length) 
                    return sql;

                return "SELECT TOP " + take + " " + sql.Substring("SELECT".Length, sql.Length - "SELECT".Length);
            }
                                    
            var orderBy = !String.IsNullOrEmpty(OrderByExpression)
                ? OrderByExpression
	            : BuildOrderByIdExpression();

            var tableName = OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDef).Trim();

            sql = this.BuildDeclareFirstIdExpression();
            sql += this.BuildSelectTopNIdExpression(skip) + " FROM " + tableName + ";";


            var where = String.IsNullOrWhiteSpace(this.WhereExpression);

            sql += String.Join(" ",
                SelectExpression.Remove(SelectExpression.IndexOf("FROM")).Trim(),
                " FROM " + tableName,
                this.BuildWhereIdGTEFirstId(),
                GroupByExpression,
                HavingExpression).Trim();

	        return sql;
        }

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            var setFields = new StringBuilder();
            var dialectProvider = OrmLiteConfig.DialectProvider;

            foreach (var fieldDef in ModelDef.FieldDefinitions)
            {
                if (UpdateFields.Count > 0 && !UpdateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement) 
                    continue;

                var value = fieldDef.GetValue(item);
                if (excludeDefaults && TypeHelper.IsDefaultValue(value)) 
                    continue;

                fieldDef.GetQuotedValue(item);

                if (setFields.Length > 0) setFields.Append(",");
                setFields.AppendFormat("{0} = {1}",
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            return string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(ModelDef), setFields, WhereExpression);
        }

        protected virtual void AssertValidSkipRowValues()
        {
            if (Skip.HasValue && Skip.Value < 0)
                throw new ArgumentException(String.Format("Skip value:'{0}' must be>=0", Skip.Value));

            if (Rows.HasValue && Rows.Value <0)
                throw new ArgumentException(string.Format("Rows value:'{0}' must be>=0", Rows.Value));
        }

	    protected virtual string BuildOrderByIdExpression()
	    {
	        if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");
            
	        return String.Format("ORDER BY {0}", ModelDef.PrimaryKey.FieldName);
	    }

        protected virtual string BuildWhereIdGTEFirstId()
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");
            
            var where = String.IsNullOrWhiteSpace(WhereExpression)
                ? String.Format("{0} > @first_id", ModelDef.PrimaryKey.FieldName)
                : String.Format("({0}) AND {1} > @first_id", WhereExpression.Trim().Substring("WHERE".Length), ModelDef.PrimaryKey.FieldName);

            return "WHERE " + where;
        }

        protected virtual string BuildSelectTopNIdExpression(int skip)
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

            return String.Format("SELECT TOP {0} {1} ", skip - 1, ModelDef.PrimaryKey.FieldName);
        }

        protected virtual string BuildDeclareFirstIdExpression()
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

            return String.Format("DECLARE @first_id {0};", 
                OrmLiteConfig.DialectProvider.GetColumnTypeDefinition(ModelDef.PrimaryKey.FieldType));
        }

		public override string LimitExpression
		{
			get
			{
				return "";
			}
		}
	}
}