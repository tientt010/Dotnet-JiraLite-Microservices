using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiraLite.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPg_trgmExtension : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable extensions
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION immutable_unaccent(text)
            RETURNS text AS $$
                SELECT unaccent($1);
            $$ LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT;
            """);

        migrationBuilder.Sql("""
            CREATE INDEX idx_issues_title_search
            ON "Issues" USING gin (immutable_unaccent("Title") gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX idx_issues_description_search
            ON "Issues" USING gin (immutable_unaccent("Description") gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX idx_projects_name_search
            ON "Projects" USING gin (immutable_unaccent("Name") gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX idx_projects_description_search
            ON "Projects" USING gin (immutable_unaccent("Description") gin_trgm_ops);
            """);

        migrationBuilder.Sql("""
            CREATE INDEX idx_issuechangelogs_description_search
            ON "IssueChangeLogs" USING gin (immutable_unaccent("Description") gin_trgm_ops);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_issues_title_search;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_issues_description_search;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_projects_name_search;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_projects_description_search;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_issuechangelogs_description_search;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS immutable_unaccent(text);");
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS unaccent;");
    }
}
