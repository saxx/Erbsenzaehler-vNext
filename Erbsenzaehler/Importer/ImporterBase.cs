﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Erbsenzaehler.Models;
using Erbsenzaehler.Rules;

namespace Erbsenzaehler.Importer
{
    public abstract class ImporterBase : CsvReader
    {
        protected ImporterBase(TextReader reader) : base(reader)
        {
        }


        public async Task<ImportResult> LoadFileAndImport(Db db, Client client, Account account, RulesApplier rulesApplier)
        {
            var result = new ImportResult();
            var lines = GetRecords<Line>();

            var anyChangesToDatabase = false;
            foreach (var line in lines)
            {
                var lineExists = db.Lines.Any(x => x.AccountId == account.Id &&
                                                   x.OriginalDate == line.OriginalDate &&
                                                   x.OriginalText == line.OriginalText &&
                                                   // ReSharper disable once CompareOfFloatsByEqualityOperator
                                                   x.OriginalAmount == line.OriginalAmount);

                if (!lineExists)
                {
                    line.AccountId = account.Id;

                    await rulesApplier.Apply(db, client, line);

                    db.Lines.Add(line);
                    result.NewLinesCount++;
                    anyChangesToDatabase = true;
                }
                else
                {
                    result.DuplicateLinesCount++;
                }
            }

            if (anyChangesToDatabase)
            {
                await db.SaveChangesAsync();
            }

            return result;
        }


        public class ImportResult
        {
            public int NewLinesCount { get; set; }
            public int DuplicateLinesCount { get; set; }
        }
    }
}