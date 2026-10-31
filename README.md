# XboxLag

Fundação de um projeto gratuito e open source em C#/.NET 10 LTS para otimização
de rotas de jogos no Windows 11. Roteamento, relay UDP e interface gráfica ainda
não foram implementados.

## Requisitos

- SDK .NET 10.0.403 (ou patch posterior da mesma feature band, conforme global.json).
- Git; Windows ou Linux para esta solução de bibliotecas independente de plataforma.
- Semgrep OSS 1.182.1 e Trivy 0.76.1; no Windows, os comandos abaixo podem rodar
  no Ubuntu WSL2. Docker e autenticação paga não são necessários.

## Estrutura

```text
XboxLag/
├── .github/workflows/ci.yml
├── .semgrep/csharp.yml
├── .editorconfig
├── .gitignore
├── CONTRIBUTING.md
├── Directory.Build.props
├── README.md
├── XboxLag.slnx
├── coverage.runsettings
├── global.json
├── trivy.yaml
├── docs/
│   ├── quality-security.md
│   └── validation-stage-01.md
├── docker/                         # vazio; nenhuma imagem nesta etapa
├── src/
│   ├── XboxLag.Domain/
│   ├── XboxLag.Application/
│   └── XboxLag.Infrastructure/
└── tests/
    ├── XboxLag.Domain.Tests/
    ├── XboxLag.Application.Tests/
    └── XboxLag.Infrastructure.Tests/
```

Cada diretório de projeto contém seu `.csproj` e `packages.lock.json` versionado.
Dependências de produção: Infrastructure → Application → Domain. Cada projeto
de testes referencia somente sua camada de produção; referências transitivas
seguem a mesma arquitetura.

## Desenvolvimento

Execute na raiz:

```powershell
dotnet restore XboxLag.slnx --locked-mode
dotnet build XboxLag.slnx -c Release --no-restore
dotnet test XboxLag.slnx -c Release
dotnet format XboxLag.slnx --verify-no-changes
```

Para aplicar formatação, execute `dotnet format XboxLag.slnx` e revise as alterações.
Ao alterar pacotes intencionalmente, execute `dotnet restore XboxLag.slnx --force-evaluate`
e revise os lockfiles junto com os projetos.

## Cobertura

```powershell
dotnet test XboxLag.slnx -c Release --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory artifacts/test-results --logger trx
```

Os XMLs Cobertura ficam em `artifacts/test-results/<id>/coverage.cobertura.xml`.
Abra os arquivos no editor ou em um visualizador compatível. No GitHub Actions,
baixe o artifact `test-results-and-coverage` na execução do workflow.
Não há percentual mínimo: ainda não existe código de negócio nem testes reais.
O runner informa ausência de testes; isso não significa testes aprovados.
XMLs vazios eventualmente produzidos pelo collector não representam cobertura útil.

Consulte [qualidade e segurança](docs/quality-security.md),
[contribuição](CONTRIBUTING.md) e [validações desta etapa](docs/validation-stage-01.md).
