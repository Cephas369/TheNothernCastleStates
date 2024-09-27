<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:output omit-xml-declaration="yes"/>

    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="//CraftingTemplate[@id='OneHandedSword']//UsablePiece[not(following-sibling::UsablePiece)]">
		<xsl:copy-of select="."/> 
        <UsablePiece piece_id="tncs_battania_noble_blade_1"/>
    </xsl:template>
	
	<xsl:template match="//CraftingTemplate[@id='TwoHandedSword']//UsablePiece[not(following-sibling::UsablePiece)]">
		<xsl:copy-of select="."/> 
        <UsablePiece piece_id="tncs_vlandian_blade_3"/>
    </xsl:template>
    

    <xsl:template match="//CraftingTemplate[@id='TwoHandedPolearm']//UsablePiece[not(following-sibling::UsablePiece)]">
        <xsl:copy-of select="."/>
        <UsablePiece piece_id="tncs_spear_blade_13"/>
    </xsl:template>

    <xsl:template match="//CraftingTemplate[@id='OneHandedAxe']//UsablePiece[not(following-sibling::UsablePiece)]">
        <xsl:copy-of select="."/>
        <UsablePiece piece_id="tncs_sickle_blade_1"/>
    </xsl:template>

    <xsl:template match="//CraftingTemplate[@id='TwoHandedAxe']//UsablePiece[not(following-sibling::UsablePiece)]">
        <xsl:copy-of select="."/>
		<UsablePiece piece_id="tncs_axe_craft_38_head"/>
    </xsl:template>
    
	
	<xsl:template match="//CraftingTemplate[@id='Javelin']//UsablePiece[not(following-sibling::UsablePiece)]">
        <xsl:copy-of select="."/>
        <UsablePiece piece_id="tncs_spear_blade_27"/>
        <UsablePiece piece_id="tncs_spear_blade_15"/>
    </xsl:template>
</xsl:stylesheet>
