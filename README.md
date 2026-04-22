# GoodHamburger

---

## Decisões técnicas

-EFCore e SQLite
Adicionar o motivo da escolha do EFCore e SQLite, além do trade-off envolvido.
Esqueci que, por padrão, o SQLite é case sensitive e accent sensitive, e não é possível alterar essa configuração. A melhor solução, nesse caso, seria migrar para MySQL, SQL Server ou outro banco que suporte a collation ci_ai.

-Repository e Service Patterns
Motivo da escolha: seguir a documentação da Microsoft e facilitar os testes unitários de cada funcionalidade.

-ProdutoEntity e polimorfismo
Para impressionar, cogitei usar polimorfismo em ProdutoEntity, mas ao analisar melhor percebi que não adicionaria nenhum campo extra nas heranças. Eu estava complicando demais, então decidi usar um ENUM para representar o tipo.
Com isso, também não foi mais necessário utilizar o Newtonsoft.Json para manter o polimorfismo das requisições json da API.

-Autenticação com Identity
Utilizei o Identity para autenticação na API, evitando criar manualmente tabelas de roles, regras de validação de senha e e-mail. Isso me permitiu adicionar segurança à aplicação sem investir muito tempo em algo já bem consolidado (como o Identity da Duende).

-Extension Methods
Usei extension methods para não poluir as classes de entidades e models, seguindo o princípio da responsabilidade única: a entidade é entidade, o model é model, e os métodos ficam como estáticos ou em services.

-Models e DTOs
Optei por usar Models e DtoRequest. Devido ao tamanho da aplicação, não é necessário ter DTOs para cada retorno da API no momento, mas conforme a aplicação crescer, será uma boa prática. Já para requisições, é sempre bom enviar apenas o necessário para a API, deixando claro para quem for implementar o que exatamente deve ser enviado.