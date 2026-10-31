# Política de qualidade e segurança

## .NET e análise estática

`Directory.Build.props` centraliza nullable e implicit usings, Roslyn do próprio
SDK, `AnalysisLevel=10.0-recommended`, estilo no build e warnings como erros.
Os projetos mantêm `net10.0`. Não é necessário adicionar um pacote de analyzers
equivalente ao já distribuído pelo SDK. Os testes também recebem xunit.analyzers
transitivamente. `.editorconfig` preserva o estilo existente, exige formatação e
nomenclatura (IDE0055/IDE1006), usings úteis (IDE0005) e corrige a seleção de
parâmetros genéricos. Não há supressões de segurança.

## NuGet Audit

```powershell
dotnet restore XboxLag.slnx --locked-mode --force
dotnet list XboxLag.slnx package --vulnerable --include-transitive
```

O restore é o gate: `NuGetAudit=true`, `NuGetAuditMode=all` e `NuGetAuditLevel=low`.
Todas as severidades (LOW, MODERATE, HIGH e CRITICAL) bloqueiam, pois os avisos
NU1901–NU1904 são erros. Falhas de consulta (NU1900) também não são ocultadas.
O comando de listagem é um diagnóstico adicional e não substitui esse gate.
Os lockfiles registram dependências transitivas e hashes; o CI usa `--locked-mode`.

Referência: [NuGet Audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages).

## Semgrep OSS

No Linux/WSL, na raiz do checkout:

```bash
semgrep --version
semgrep scan --config .semgrep/csharp.yml --error --strict --metrics=off --disable-version-check .
```

Versão fixada: 1.182.1. Sem login e sem regras remotas mutáveis. As duas regras
locais focam bypass explícito de TLS e criação de BinaryFormatter. Todo achado
dessas regras ERROR bloqueia via `--error`; configuração inválida falha via
`--strict`. Os resultados aparecem no log. Este conjunto inicial é deliberadamente
limitado e não representa cobertura completa de segurança C#. Deve crescer com
os riscos concretos do Route Optimizer. Sem arquivos C#, o scan termina sem alvos.

## Trivy

Execute somente após sucesso do Semgrep, na mesma raiz Linux/WSL:

```bash
trivy --version
trivy image --download-db-only
trivy fs --config trivy.yaml .
```

Versão fixada: 0.76.1. `image --download-db-only` apenas atualiza o banco, sem
Docker ou análise de imagem. O scan de filesystem analisa vulnerabilidades NuGet
pelos lockfiles, configurações suportadas e segredos da árvore atual. Não analisa
o histórico Git. HIGH e CRITICAL bloqueiam, inclusive segredos e configurações
nessas severidades. LOW/MEDIUM não são bloqueantes neste scan; NuGet Audit mantém
a política mais estrita para pacotes. Não existe `ignore-unfixed` nem ignorefile.
`.git`, artifacts e saídas bin/obj são excluídos como metadados/saídas geradas;
código fonte, workflows e lockfiles permanecem no escopo.

Ainda não existem Dockerfiles ou outros arquivos de infraestrutura reconhecidos
pelo Trivy: zero configurações elegíveis não equivale a configurações aprovadas.
Futuramente acrescente um job de imagem dependente do Semgrep, com versão/digest
da imagem explícitos e a mesma política, depois de existir uma imagem real.

Referência: [suporte .NET do Trivy](https://trivy.dev/docs/v0.76/guide/coverage/language/dotnet/).

## GitHub Actions

`ci.yml` roda em PRs para main, pushes na main e manualmente. O job quality faz
restore auditado, um build Release com analyzers, formatação e testes com
Coverlet sem recompilar. Publica TRX e Cobertura por 14 dias quando existem.
O job Semgrep instala a versão fixa do PyPI oficial. O job Trivy exige sucesso
do Semgrep (`needs: semgrep`), instala o binário fixado usando a action oficial
cujo script e checksum são verificados e atualiza o banco antes de analisar.

Todas as actions diretas estão fixadas por SHA completo, consultado no upstream.
O token tem apenas contents:read; checkout não persiste credenciais. Não há
secrets obrigatórios, pull_request_target ou continue-on-error. Concorrência
cancela execuções redundantes por PR/ref. Ubuntu 24.04 é suficiente para as
bibliotecas atuais; um runner Windows será necessário quando houver código
dependente do Windows. Cache NuGet usa os lockfiles; banco Trivy não é congelado.

Após publicação autorizada, valide uma execução real e configure proteção da
main para exigir os três jobs (configuração do repositório, fora dos arquivos).
Workflow localmente revisado não equivale a workflow remoto aprovado.

## Exceções

Nenhuma exceção ou supressão foi adicionada. Uma futura exceção requer justificativa,
identificador do achado, escopo mínimo, responsável, prazo e aprovação em revisão.
Nunca desabilite um scanner para obter sucesso no pipeline.
