# Validação da etapa 01

Execução local em 31/10/2026, branch `chore/quality-ci-foundation` inicialmente
limpa. SDK 10.0.403, Windows; segurança via Ubuntu WSL2. Nenhum commit, merge,
push ou tag executado. A solução original e suas dependências foram preservadas.

| Comando | Resultado e observações |
| --- | --- |
| `dotnet restore XboxLag.slnx` | Saída 0. Sem warnings ou erros. |
| `dotnet restore XboxLag.slnx --locked-mode --force` | Saída 0 nos seis projetos; NuGet Audit ativo para diretas e transitivas, todas as severidades bloqueantes. |
| `dotnet list XboxLag.slnx package --vulnerable --include-transitive` | Saída 0; nenhum pacote vulnerável nas fontes consultadas. |
| `dotnet build XboxLag.slnx -c Release --no-restore` | Saída 0; seis projetos compilados, zero warnings e zero erros; Roslyn habilitado. |
| `dotnet test XboxLag.slnx -c Release` | Runner executado; nenhum teste disponível nos três assemblies. Não representa testes de negócio aprovados. |
| `dotnet test XboxLag.slnx -c Release --no-build --no-restore --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory artifacts/test-results --logger trx` | Saída 0; três relatórios Cobertura e TRX produzidos; sem testes, cobertura não representativa. |
| `dotnet format XboxLag.slnx --verify-no-changes` | Saída 0; nenhuma alteração necessária. Não há arquivos C# de produção nesta etapa. |
| `semgrep scan --config .semgrep/csharp.yml --error --strict --metrics=off --disable-version-check .` | Semgrep 1.182.1, saída 0; duas regras, zero alvos C#, zero achados. |
| `semgrep scan --config .semgrep/csharp.yml --error --strict --metrics=off --disable-version-check --no-git-ignore artifacts/security-rule-check/Unsafe.cs` | Prova temporária das regras: saída 1 esperada, quatro achados bloqueantes para três bypasses TLS e uma criação BinaryFormatter. Fixture ignorada pelo Git e fora dos projetos; não é teste de negócio nem contribui para cobertura. |
| `trivy image --download-db-only` seguido de `trivy fs --config trivy.yaml .` | Trivy 0.76.1, saída 0; três lockfiles com dependências, zero vulnerabilidades HIGH/CRITICAL; nenhum segredo bloqueante reportado. Zero arquivos de configuração elegíveis. Banco atualizado em 31/10/2026 às 01:14:32 UTC. |

## Configurações e pacotes

Cada projeto de testes usa Microsoft.NET.Test.Sdk 18.11.0, xunit 2.9.3,
xunit.runner.visualstudio 4.0.1 e coverlet.collector 10.1.0, versões estáveis
consultadas no NuGet. Nenhum pacote adicionado aos projetos de produção.
Os seis lockfiles estão versionáveis para reprodução e inspeção pelo Trivy.
Nullable e usings foram centralizados sem duplicação nos projetos.

## Arquivos

Criados: `.github/workflows/ci.yml`, `.semgrep/csharp.yml`, `Directory.Build.props`,
`global.json`, `coverage.runsettings`, `trivy.yaml`, `README.md`, `CONTRIBUTING.md`,
`docs/quality-security.md`, este relatório, três `.csproj` em `tests/` e seis
`packages.lock.json` (um por projeto).

Modificados: `.editorconfig`, `XboxLag.slnx` e os três `.csproj` de produção.
Árvore final no README. Conteúdo integral disponível nos próprios arquivos;
uma cópia consolidada para revisão local fica em `artifacts/stage-01-files.md`
(gerada, não versionada), sem fragmentos ou diffs.

## Limitações e pendências

- O sandbox inicialmente bloqueou WSL e pipes internos do .NET; a repetição com
  execução autorizada fora do sandbox permitiu as validações acima. A primeira
  falha de build/format não foi considerada aprovação.
- Sem lógica C# não há testes reais; percentuais de cobertura vazios não são
  indicadores de qualidade. O SAST foi verificado com fixture isolada, mas o scan
  da solução ainda não tem código alvo.
- As regras Semgrep iniciais têm escopo limitado. Trivy não substitui auditoria
  de histórico de segredos e não encontrou configurações de infraestrutura para analisar.
- Workflow revisado localmente; execução no GitHub permanece pendente de autorização
  para publicar a branch. Não se declara aceite remoto concluído. A proteção da main
  e exigência dos jobs também dependem de configuração no GitHub.
- Nenhuma funcionalidade de roteamento, UDP ou Avalonia foi implementada.

Conventional Commit sugerido:
`chore(ci): establish quality and security foundation`.
