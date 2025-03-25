import discord
from discord.ext import commands
from discord import app_commands

class Client(commands.Bot):
    async def on_ready(self):
        print(f'Logged on as {self.user}!')

        try:
            guild = discord.Object(id=1347249457514811453)
            synced = await self.tree.sync(guild=guild)
            print(f'Synced {len(synced)} commands to guild {guild.id}')
        except Exception as e:
            print(f'Error syncing commands: {e}')


    async def on_message(self, message):
        if message.author == self.user:
            return
        if message.content.startswith(''):
            for _ in range(1):
                await message.channel.send(f'')


intents = discord.Intents.default()
intents.message_content = True
client = Client(command_prefix="K#", intents=intents)


GUILD_ID = discord.Object(id=1347249457514811453)

@client.tree.command(name="tinw", description = "wefuweniuwenuqwbnfiuewbnf", guild=GUILD_ID)
async def tinw(interaction: discord.Interaction):
    await interaction.response.send_message("ergierguerhubruogberuobgwgb")

