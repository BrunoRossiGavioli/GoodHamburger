# GoodHamburger

---

## Decisões técnicas - API

### 1. EFCore + SQLite

**Decisão:** Utilizar EFCore como ORM e SQLite como banco de dados durante o desenvolvimento inicial.

**Motivo:** Agilidade na configuração e prototipagem, sem necessidade de servidor de banco dedicado.

**Trade-off identificado:**  
O SQLite é, por padrão, **case sensitive** e **accent sensitive** (`cs` e `as`). Essa configuração não pode ser alterada após a criação do banco.  
Isso pode gerar comportamentos inesperados em buscas textuais (ex: `"nome" = "João"` não encontrar `"joão"`).

**Alternativa futura:**  
Migrar para **MySQL** ou **SQL Server**, que suportam collation `ci_ai` (case insensitive e accent insensitive), garantindo buscas mais previsíveis e compatíveis com regras de negócio comuns.

---

### 2. Repository Pattern + Service Pattern

**Decisão:** Adotar os padrões Repository e Service.

**Motivo:**  
- Seguir as recomendações da documentação oficial da Microsoft para ASP.NET Core.  
- Separar a lógica de acesso a dados (Repository) da lógica de negócio (Service).  
- Facilitar a aplicação de **testes unitários** por meio de injeção de dependência e mocks.

**Benefício direto:** Cada funcionalidade pode ser testada isoladamente sem depender de banco real ou integrações externas.

---

### 3. ProdutoEntity – Polimorfismo vs Enum

**Decisão:** Utilizar um `enum` para representar o tipo do produto, em vez de herança/polimorfismo.

**Motivo inicial para polimorfismo:** Tentativa de demonstrar conhecimento avançado de orientação a objetos.

**Análise crítica:**  
Ao avaliar melhor, percebi que nenhuma das subclasses adicionaria campos extras ou comportamentos significativamente diferentes. O uso de herança seria desnecessário e geraria complexidade sem ganho real.

**Decisão final:**  
- Uso de `enum TipoProduto`.  
- Código mais simples, direto e fácil de manter.  
- Não foi necessário usar **Newtonsoft.Json** para serialização polimórfica.

> *"Princípio KISS (Keep It ~~Simple, Stupid~~ Super Simple) aplicado."*

---

### 4. Autenticação com Identity (Duende)

**Decisão:** Utilizar o ASP.NET Core Identity para gerenciar autenticação e autorização.

**Motivo:**  
- Evitar reimplementar funcionalidades complexas como:  
  - Tabelas de usuários, roles e claims  
  - Validação de senha e e-mail  
  - Recuperação de senha, confirmação de e-mail, etc.  
- Adicionar segurança de forma consistente e confiável.  
- Não investir tempo reinventando a roda em um sistema bem consolidado (Identity da Duende).

---

### 5. Extension Methods

**Decisão:** Utilizar extension methods para transformações e mapeamentos auxiliares.

**Motivo:**  
- Respeitar o **Princípio da Responsabilidade Única (SRP)**.  
- Evitar poluir as classes de entidades e models com métodos de conversão ou lógica auxiliar.  
- Manter entidades focadas apenas no estado, e models focados na representação de dados.

**Organização resultante:**  
- Entidade = apenas dados persistidos.  
- Model/DTO = apenas dados de transporte.  
- Extension methods = lógica estática e reutilizável.

---

### 6. Uso de Models e DTOs

**Decisão:**  
- Criar **DtoRequest** para todas as requisições da API.  
- Utilizar diretamente os **Models** como retorno da API (sem DTOs específicos de resposta no momento).

**Justificativa para DTOs de requisição:**  
- Documentam claramente o que o cliente precisa enviar.  
- Evitam envio de campos desnecessários ou sensíveis.  
- Facilitam a validação de entrada.

**Justificativa para não usar DTOs de resposta ainda:**  
- Tamanho atual da aplicação não exige essa camada adicional.  
- À medida que a aplicação crescer e os retornos se tornarem mais complexos, será natural introduzir DTOs de resposta.  
- Evita over-engineering no momento atual.

> *"YAGNI (You Ain't Gonna Need It) aplicado para respostas, mas não para requisições."*

<br/>

### Resumo dos Trade-offs Assumidos

| Decisão | Trade-off assumido |
|---------|--------------------|
| SQLite | Case/accent sensitive – futuro: migrar para MySQL/SQL Server |
| Sem DTO de resposta | Menos flexibilidade futura, mas menos complexidade atual |
| Enum vs herança | Menos "flexibilidade teórica", mais simplicidade e clareza |

---

## Definições técnicas - Portal em Blazor

Criei um projeto Blazor App com renderização em Server, escolhi a versão server pois o WebAssembly demora um pouco na primeira inicialização e tem foco em aplicações que podem rodar por pequenos e médios periodos offline, o que não é o caso do portal da GoodHamburger.

Escolhi o MudBlazor pois é um dos pacotes de componentes gratituidos dos quais eu já tive previa experiência, e tem uma boa quantidade de componentes para o que eu ia precisar no portal.

Havia estruturado anteriormente que eu gostaria de para o MVP do portal ter o seguinte:
- autenticação e autorização, consumindo da API
- A página principal do portal seria o painel de gerar pedido.
- Ter os cadastros auxiliares de cliente e produto, para dar um ar de aplicação mais realista
	-Obs.: Não tive tempo para implementar no cadastro de produto a parte de versionamento de valores, a estrutura existe na API
- Ter página para listagem de pedidos e também detalhamento
- Alterar o status do pedido
	-Não defini nenhum workflow específico para isso, então optei por permitir que o usuário alterasse o status para qualquer outro, sem restrições.
- Havia planejado de utilizar o FluentValidation com Blazilla para a aplicação, porém utilizei DataAnnotations nos formulário de cadastro e edição junto a uma model interna da .razor, eu evitaria fazer isso em um projeto real, mas para economizar tempo de uma coisa bonûs optei por utilizar um pouco de GoHorse



Como já havia rascunhado e estruturado mais ou menos o que precisaria para o portal, utilizei vários agentes de IA
	- Anthropic Claude Code: 
		- Haiku 4.5 para tarefas mais simples, como gerar os scaffolds da páginas.
		- Opus 4.6 para identificar possíveis problema em runtime, foquei em arquivos especificos então é possível que existam outros problemas que não foram identificados.
	- Anthropic Claude.Ai desktop, utilizei para tarefas que levariam mais tempo e poderiam ser executadas em segundo plano ou até em nuvem, mas foi bem pontual, acho utilizei poucas vezes para esse projeto.
	- DeepSeek: Utilizei ele para melhoria de prompts e gerar instruções claras para o Haiku, além de ajustes mais bobos que comeriam muito token dos agente da Anthropic
	-Obs.: Sempre utilizo a IA como ferramenta e assistente, é ótima para tarefas manuais como preparar um seed para banco de dados e scaffold, para o portal gostaria de ter colocado mais a mão na massa, mas ter um MVP tive que abrir mão um pouco da qualidade

Sobre a interface, de fato não está bonita, funciona, mas nem em sonhos entregaria alguma assim para um cliente.

Falando um pouco sobre a estrutura da solicitação de pedidos, utilizei EventCallBacks como prop nos componentes para a lógica de cadastro e removação ficar dentro do componente pai (Home.razor), os agentes geram uma quantidade de propriedades desnecessárias, mas seguiu os principios planejei, alguns callbacks poderia ser simplicados, mas não vejo como um problema só como um código não muito pensado pelo agente, fiz os ajustes necessário para nada quebrar e funcionar.
Minha ideia era ser algo parecido com aqueles sites de hambueria pequena, cardápio e carrinho.