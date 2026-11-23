using System;
using System.Collections.Generic;
using System.Linq;

namespace SamsLiquorShop
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Excerpt { get; set; }
        public string Image { get; set; }
        public string[] Paragraphs { get; set; }
    }

    public partial class Blog : System.Web.UI.Page
    {
        public static readonly List<BlogPost> Posts = new List<BlogPost>
        {
            new BlogPost
            {
                Id = 1,
                Title = "5 Wines to Try This Summer",
                Category = "Wine",
                Excerpt = "Our top picks for warm Durban evenings, from crisp whites to light rosés.",
                Image = "Images/blog/post1.jpg",
                Paragraphs = new[]
                {
                    "Durban summers ask a lot of a wine. It has to be cold, it has to be easy to drink, and it should still taste like something after twenty minutes in the sun. These are the five styles we keep reaching for.",
                    "Start with a Sauvignon Blanc if you like it sharp and zesty, or a Chenin Blanc if you want a bit more fruit. Both are great with seafood and salads. A dry rosé is the safe choice for a crowd, and a chilled sparkling wine never needs an excuse. If you're a red drinker, try a lighter one like a Pinot Noir and pop it in the fridge for fifteen minutes first.",
                    "Serve everything colder than you think you should. Ice in a bucket beats ice in the glass, and a shady spot beats a sunny one. You'll find all five styles in our wine range."
                }
            },
            new BlogPost
            {
                Id = 2,
                Title = "How to Host the Perfect Braai",
                Category = "Entertaining",
                Excerpt = "Drinks pairings, timing tips, and a playlist to keep the evening going.",
                Image = "Images/blog/post2.jpg",
                Paragraphs = new[]
                {
                    "A good braai is mostly about timing. Get the fire going early, let the coals burn down to a steady glow, and don't start cooking until everyone has a drink in their hand.",
                    "For drinks, keep it simple. Cold beer and cider for the afternoon, a crisp white or rosé for the table, and something darker like a Pinotage or a bourbon and cola once the sun goes down. Fill the cooler with ice before your guests arrive, not after, and always have more water than you think you need.",
                    "Put someone you trust on the tongs, build a playlist everyone can live with, and don't forget the non-drinkers. A jug of iced tea with mint goes down well. Most of all, make sure everyone has a safe way home."
                }
            },
            new BlogPost
            {
                Id = 3,
                Title = "Behind the Bar: Meet Our Spirits Buyer",
                Category = "Behind the Bar",
                Excerpt = "We sat down with our head buyer to talk rare finds and what's coming next.",
                Image = "Images/blog/post3.jpg",
                Paragraphs = new[]
                {
                    "Every bottle on our shelves started as a conversation. Our spirits buyer spends a surprising amount of time tasting, comparing and saying no, so that what makes it onto the shelf is worth your money.",
                    "The job is part detective work and part patience. It means chasing down small-batch whiskies and local gins before they sell out, and keeping an eye on what customers keep asking for. Lately that's been smoky single malts, agave spirits and gins made with local botanicals.",
                    "What's coming next? A few new arrivals we're excited about, plus some rare finds that won't be around for long. Join the newsletter and you'll hear about them first."
                }
            },
            new BlogPost
            {
                Id = 4,
                Title = "Gin 101: A Beginner's Guide",
                Category = "Spirits",
                Excerpt = "Botanicals, styles, and how to build your first proper gin and tonic.",
                Image = "Images/blog/post4.jpg",
                Paragraphs = new[]
                {
                    "Gin is a neutral spirit flavoured with botanicals, and the main one has to be juniper. Everything else, like citrus peel, coriander seed, rooibos or fynbos, is up to the distiller. That's why no two gins taste quite the same.",
                    "If you're just starting out, try a London Dry for the classic pine and citrus taste, then a more modern style if you like floral or fruity flavours. Local gins are worth a look too, since many use South African botanicals you won't find anywhere else.",
                    "To build your first proper gin and tonic, fill a big glass with ice and pour in a single measure of gin. Top with cold tonic, about three parts tonic to one part gin, and finish with a slice of citrus or a sprig of rosemary. Stir once, gently, and you're done."
                }
            },
            new BlogPost
            {
                Id = 5,
                Title = "Low & No: Our Favourite Alcohol-Free Picks",
                Category = "Low & No Alcohol",
                Excerpt = "Great taste doesn't need proof — our top zero-proof serves for any occasion.",
                Image = "Images/blog/post5.jpg",
                Paragraphs = new[]
                {
                    "Not drinking doesn't have to mean boring drinking. Alcohol-free options have come a long way, and some are good enough that you'll forget what's missing.",
                    "Look for alcohol-free spirits that take their cue from gin or whisky, dry sparkling wines, and craft beers that taste like the real thing. A good tonic with a squeeze of lime, or ginger beer over ice with fresh mint, is a perfectly grown-up drink on its own.",
                    "Whether you're driving, taking a month off or just pacing yourself, there's something here for you. Ask in store and we'll point you to the best ones."
                }
            }
        };

        // The article being read (null = show the list of posts)
        protected BlogPost Current;

        protected void Page_Load(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(Request.QueryString["post"], out id))
            {
                Current = Posts.FirstOrDefault(p => p.Id == id);
            }

            if (Current != null)
            {
                Title = Current.Title + " | Sam's Liquor Shop";
            }
        }
    }
}