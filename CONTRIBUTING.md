# Contribuindo com XboxLag

1. Abra uma issue para mudanças amplas e combine o escopo.
2. Crie uma branch a partir de `main`: `feat/<descricao>`, `fix/<descricao>`,
   `chore/<descricao>` ou `docs/<descricao>`. Esta etapa usa
   `chore/quality-ci-foundation`.
3. Preserve Infrastructure → Application → Domain. Use nomes de código em inglês,
   SOLID e apenas dependências necessárias. Não coloque detalhes de infraestrutura
   no Domain. Nunca versione credenciais.
4. Adicione testes xUnit para comportamentos reais da camada alterada. Não crie
   testes vazios para obter cobertura. Execute os comandos do README e os scans
   de segurança, obrigatoriamente Semgrep antes de Trivy.
5. Revise os arquivos completos, os lockfiles e os resultados das ferramentas.
   Explique limitações e exceções no PR. Abra um PR direcionado à `main` e aguarde
   revisão e todas as verificações obrigatórias.

Use Conventional Commits: `feat:`, `fix:`, `chore:`, `docs:`, `test:` ou `refactor:`,
com escopo opcional, por exemplo `chore(ci): establish quality and security checks`.
Uma alteração incompatível usa `!` e/ou rodapé `BREAKING CHANGE:`.

O projeto adota Semantic Versioning (`MAJOR.MINOR.PATCH`): mudanças incompatíveis,
funcionalidades compatíveis e correções compatíveis, respectivamente. Na série
`0.x`, a API ainda é experimental; documente incompatibilidades. A criação de
releases e tags é uma ação explícita dos mantenedores; o CI não publica versões.

Para suspeitas de vulnerabilidade, evite divulgar credenciais ou uma exploração
em issue pública. Use o canal privado de relato do GitHub quando habilitado.
Não há canal privado configurado por esta etapa.
