# EP3 - Banco de Dados 2

## Descrição

Este projeto é a entrega do EP3 de Banco de Dados 2, desenvolvido em C# utilizando a versão 8.0.100 do .NET SDK. O projeto se conecta a um banco de dados PostgreSQL e realiza operações de consulta e manipulação de dados.

## Versão do C#

O projeto foi desenvolvido utilizando a versão 8.0.100 do .NET SDK. Para verificar a versão instalada na sua máquina, utilize o seguinte comando no terminal:

```bash
dotnet --version
```

## Compilação
Para compilar o projeto, siga os passos abaixo:

  1. Certifique-se de que você tem o .NET SDK 8.0.100 ou superior instalado na sua máquina.
  2. Navegue até o diretório do projeto.
  3. Execute o comando de build:
```bash
  dotnet build -c Release
```

## Arquivo de Configuração
Para que o projeto funcione corretamente, é necessário configurar as credenciais de acesso ao banco de dados PostgreSQL. Crie um arquivo de configuração chamado dbconfig.txt com o seguinte conteúdo:
```
Server=localhost
Port=5432
Database=MisConsultas
User Id=postgres
Password=admin
```

### Explicação dos Campos
- Server: O endereço do servidor PostgreSQL. No exemplo, estamos usando localhost para indicar que o servidor está rodando localmente.
- Port: A porta em que o servidor PostgreSQL está ouvindo. O valor padrão é 5432.
- Database: O nome do banco de dados ao qual o projeto se conectará. No exemplo, o nome do banco de dados é MisConsultas.
- User Id: O nome de usuário utilizado para se conectar ao banco de dados. No exemplo, estamos usando o usuário postgres.
- Password: A senha do usuário para autenticação no banco de dados. No exemplo, a senha é admin.

## Localização do Arquivo de Configuração
Para que o projeto possa ler as configurações corretamente, o arquivo dbconfig.txt deve estar localizado na pasta:
```
.\Entrega_EP3BD2\Projeto-EP3BD2\bin\Release\net8.0
```

## Execução
Após compilar o projeto e configurar o arquivo dbconfig.txt, você pode executar o projeto navegando até o diretório de saída localizado em 'bin' e simplesmente executar o executável - considerando que o BD já foi previamente configurado.
