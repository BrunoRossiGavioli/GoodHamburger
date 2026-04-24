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

### 4. Autenticação com Identity

**Decisão:** Utilizar o ASP.NET Core Identity para gerenciar autenticação e autorização.

**Motivo:**  
- Evitar reimplementar funcionalidades complexas como:  
  - Tabelas de usuários, roles e claims  
  - Validação de senha e e-mail  
  - Recuperação de senha, confirmação de e-mail, etc.  
- Adicionar segurança de forma consistente e confiável.  
- Não investir tempo reinventando a roda em um sistema bem consolidado.

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

---

### Resumo dos Trade-offs Assumidos (API)

| Decisão | Trade-off assumido |
|---------|--------------------|
| SQLite | Case/accent sensitive – futuro: migrar para MySQL/SQL Server |
| Sem DTO de resposta | Menos flexibilidade futura, mas menos complexidade atual |
| Enum vs herança | Menos "flexibilidade teórica", mais simplicidade e clareza |

---

## Decisões técnicas - Portal em Blazor

### Escolha do tipo de renderização

**Decisão:** Blazor Server (em vez de Blazor WebAssembly).

**Motivo:**  
O WebAssembly apresenta maior tempo de primeira inicialização (download do runtime) e é mais adequado para aplicações que precisam funcionar offline por períodos curtos ou médios. Como o portal da GoodHamburger é uma aplicação interna com acesso contínuo à internet.

---

### Biblioteca de componentes

**Decisão:** MudBlazor.

**Motivo:**  
- Gratuito e com boa documentação  
- Experiência prévia da equipe  
- Conjunto robusto de componentes (tabelas, formulários, modais, notificações) que atendia às necessidades do MVP

---

### MVP planejado x entregue

**Planejado para o MVP:**
- Autenticação e autorização consumindo a API
- Página principal com painel de geração de pedidos
- Cadastros auxiliares (cliente e produto) para dar realismo à aplicação
- Listagem de pedidos com página de detalhamento
- Alteração de status do pedido

**Entregue / Observações:**
- ✅ Autenticação integrada com a API
- ✅ Página principal com geração de pedidos
- ✅ Cadastros de cliente e produto
  - ⚠️ *Obs.:* A estrutura de versionamento de valores existe na API, mas não houve tempo para implementar no cadastro de produto do portal.
- ✅ Listagem e detalhamento de pedidos
- ✅ Alteração de status de pedido
  - ⚠️ *Obs.:* Nenhum workflow específico foi definido; optou-se por permitir alteração livre entre status, sem restrições de transição.

---

### Validação de formulários

**Decisão inicial planejada:** FluentValidation com Blazilla.

**Decisão real adotada:** DataAnnotations com model interna na `.razor`.

**Motivo da mudança:**  
Em um projeto real, utilizaria FluentValidation por oferecer maior flexibilidade e separação de concerns. No entanto, para ganhar tempo em uma funcionalidade bônus, optei por uma abordagem mais rápida ("GoHorse"), reconhecendo que não é o ideal, mas suficiente para o escopo do MVP.

---

### Uso de IA no desenvolvimento

**Ferramentas utilizadas:**

| Ferramenta | Uso |
|------------|-----|
| **Claude Code - Haiku 4.5** | Tarefas simples e repetitivas (ex: scaffolds de páginas) |
| **Claude Code - Opus 4.6** | Identificação de problemas em runtime (focado em arquivos específicos - podem existir outros problemas não identificados) |
| **Claude.ai desktop** | Tarefas demoradas executadas em segundo plano (uso pontual) |
| **DeepSeek** | Melhoria de prompts, geração de instruções claras para o Haiku, ajustes menores que consumiriam muitos tokens da Anthropic |

> **Observação importante:** A IA foi utilizada como ferramenta assistente, não como substituta do pensamento crítico. É excelente para tarefas manuais (ex: seed de banco de dados, scaffolds). Para o portal, gostaria de ter colocado mais "mão na massa", mas a necessidade de entregar um MVP me fez abrir mão parcialmente da qualidade ideal.

---

### Interface e experiência do usuário

**Avaliação honesta:**  
A interface não está bonita. Funciona, mas não seria entregue assim para um cliente real em circunstância alguma.

---

### Estrutura técnica da solicitação de pedidos

**Decisão:** Utilizar `EventCallback` como parâmetro nos componentes, mantendo a lógica de cadastro e remoção no componente pai (`Home.razor`).

**Observação crítica:**  
Os agentes de IA geraram algumas propriedades desnecessárias. Apesar disso, a estrutura seguiu os princípios planejados. Alguns callbacks poderiam ser simplificados, mas isso não configura um problema grave – apenas um código não totalmente otimizado pelo agente.

**Ajustes realizados:** Corrigi o necessário para garantir funcionamento sem quebras.

**Inspiração original do design:**  
A ideia era algo parecido com sites de hamburgueria pequena: cardápio + carrinho.

---

### Resumo dos Trade-offs assumidos (Portal)

| Decisão | Trade-off assumido |
|---------|--------------------|
| Blazor Server | Perde capacidade offline, ganha velocidade no carregamento |
| DataAnnotations (vs FluentValidation) | Menos flexibilidade, mais rapidez de implementação |
| IA intensiva nos scaffolds | Velocidade de entrega x qualidade do código gerado |
| Interface simples | Funcional x esteticamente agradável |

---

## Considerações finais

Este projeto representa um **MVP funcional** com consciência clara dos trade-offs realizados. As decisões documentadas aqui não refletem necessariamente o que seria feito em um ambiente de produção com mais tempo, mas sim um equilíbrio entre qualidade, prazo e escopo.

Áreas identificadas para melhoria futura:
- Migração do SQLite para MySQL/SQL Server
- Implementação de DTOs de resposta
- Substituição de DataAnnotations por FluentValidation
- Refinamento da interface visual