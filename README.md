# 💻 Sistema de Orçamentos - Martins Hidráulica Agriculture

Este e um sistema web desenvolvido como projeto extensionista acadêmico na faculdade UniFANAP. 

O sistema foi criado para auxiliar a empresa Martins Hidráulica Agriculture, na geração e gestão de orçamentos, serviços e clientes.

## 🎯 Objetivo do Projeto

Criar uma aplicação web completa que permita:

- Cadastro e gerenciamento de clientes e serviços
- Criação, edição, listagem e geração de PDF de orçamentos
- Controle de usuários com autenticação e permissões
- Dashboard com gráficos interativos de desempenho
- Configurações personalizaveis de orçamento
- Geração de relatórios (PDF/Excel)

## 🛠️ Tecnologias Utilizadas

- **Linguagem**: C#
- **Framework**: ASP.NET Core 9 (MVC)
- **ORM**: Entity Framework Core
- **Banco de Dados**: PostgreSQL
- **Frontend**: Razor Pages, Bootstrap, JQuery, Ajax, Tailwind CSS
- **Hospedagem**: Droplet Linux Ubuntu via DigitalOcean
- **Servidor Web**: Nginx (proxy reverso)
- **PDF**: iTextSharp 7
- **Autenticação**: ASP.NET Identity + 2FA (Otp.Net)
- **Gráficos**: Chart.js
- **Exportação Excel**: ClosedXML

## 🏗️ Estrutura do Sistema

- `Controllers`: Lógica de controle das rotas e comunicação com o banco
- `Models`: Representações das entidades do banco (Clientes, Produtos, Orçamentos, etc)
- `Views`: Interface do usuário com uso de Razor Pages
- `ViewModels`: Auxiliam na comunicação entre a View e os Models

## 🚀 Como Executar Localmente

### ✅ Pré-requisitos

Antes de iniciar, certifique-se de ter os seguintes recursos instalados em sua máquina:

1. **.NET 9 SDK**  
   [🔗 Download oficial](https://dotnet.microsoft.com/download)

2. **PostgreSQL** (instalado e configurado)  
   [🔗 Download oficial](https://www.postgresql.org/download/)

3. **IDE de sua preferência:**  
   - [Rider (JetBrains)](https://www.jetbrains.com/rider/)  
   - ou [Visual Studio](https://visualstudio.microsoft.com/)

4. **Node.js** (para instalação de bibliotecas JavaScript)  
   [🔗 Download oficial](https://nodejs.org/)

---

### 🛠️ Configuração Inicial

5. **Clone ou baixe este repositório:**

```bash
git clone https://github.com/Luqui-Araujo/MartinsHidraulicaProjetoExtensionista.git
cd MartinsHidraulicaProjetoExtensionista
```

Abra o projeto no Visual Studio ou Rider

6 - Configure a conexão com o banco no appsettings.json

```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=5432;Database=nomeDoSeuBanco;User Id=usuario;Password=suaSenha;"
}
```

7 - Execute as migrações para criar o banco de dados:

```bash
dotnet ef database update
```

8 - Crie um usuário de acesso inicial ao sistema (caso deseje testar diretamente com login)

No seu banco PostgreSQL, execute os comandos abaixo:

```bash
-- Inserção do usuário administrador
INSERT INTO "AspNetUsers" (
    "Id", "Departamento", "TipoUsuario", "Nome", "Ativo", "UserName", 
    "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
    "TwoFactorEnabled", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
    "PhoneNumber", "PhoneNumberConfirmed", "LockoutEnabled", "AccessFailedCount"
) VALUES (
    gen_random_uuid(),
    'Administrativo',
    'Administrador',
    'Teste',
    true,
    'teste@gmail.com',
    'TESTE@GMAIL.COM',
    'teste@gmail.com',
    'TESTE@GMAIL.COM',
    false,
    false,
    'AQAAAAIAAYagAAAAELlXAFB5+Z0zqYmkQi+ckP9kHEGMHQiAVEHIwRb28tMhZzEQCY22QAYYSdUqHcc5+A==',
    gen_random_uuid(),
    gen_random_uuid(),
    '62982884676',
    false,
    false,
    0
);
sql
Copiar
Editar
-- Inserção da role de administrador
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName") 
VALUES (gen_random_uuid(), 'Administrador', 'ADMINISTRADOR');

-- Associe o usuário à role criada (substitua pelos respectivos IDs gerados)
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") 
VALUES ('ID_DO_USUARIO', 'ID_DA_ROLE');
```

## ▶️ Executando o Sistema

9 - No terminal, execute o projeto:

```bash
dotnet run
```

Abra o navegador e acesse:
http://localhost:5000

10 - Faça login com as seguintes credenciais de teste:

E-mail: teste@gmail.com

Senha: SenhaForte123!

🧠 Aprendizados

- Este projeto permitiu aplicar e consolidar diversos conceitos como:
  
Estrutura MVC e boas práticas com C#
Criação de componentes reutilizáveis com Razor
Integração com banco de dados relacional via Entity Framework
Controle de autenticação e autorização de usuários
Deploy de aplicação ASP.NET Core com Nginx em servidor Linux
Criação de PDFs dinâmicos e exportação de relatórios

