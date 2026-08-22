<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SamsLiquorShop._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="hero-wrap">
      <div class="glass hero-card">
        <div class="hero-text">
          <p class="hero-eyebrow">Sam's Liquor Shop</p>
          <h1 class="hero-heading">Good times<br>start here.</h1>
          <p class="hero-sub">Discover your favourites, explore something new, and enjoy great drinks at great prices.</p>
          <div class="hero-buttons">
            <a href="Shop.aspx" class="btn-primary">Shop Now</a>
            <a href="Specials.aspx" class="btn-ghost">View Specials</a>
          </div>
        </div>
        <div class="hero-image-wrap">
          <img src="Images/hero-drink.jpg" alt="Featured drinks" class="hero-image" />
        </div>
      </div>
    </section>

</asp:Content>
