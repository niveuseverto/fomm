﻿using System;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools.GraphicsSettings
{
  public partial class OverrideSlider : UserControl
  {
    public Int32 m_intDivisor = 1;

    public OverrideSlider()
    {
      InitializeComponent();
    }

    public Int32 Minimum
    {
      get
      {
        return tkbSlider.Minimum/m_intDivisor;
      }
      set
      {
        tkbSlider.Minimum = value*m_intDivisor;
      }
    }

    public Int32 Maximum
    {
      get
      {
        return tkbSlider.Maximum/m_intDivisor;
      }
      set
      {
        tkbSlider.Maximum = value*m_intDivisor;
      }
    }

    public Int32 TickFrequency
    {
      get
      {
        return tkbSlider.TickFrequency;
      }
      set
      {
        tkbSlider.TickFrequency = value;
      }
    }

    public Int32 Divisor
    {
      get
      {
        return m_intDivisor;
      }
      set
      {
        if (value == 0)
        {
          return;
        }
        var intMax = Maximum;
        var intMin = Minimum;
        m_intDivisor = value;
        Maximum = intMax;
        Minimum = intMin;
        nudValue.DecimalPlaces = m_intDivisor > 1 ? 1 : 0;
      }
    }

    public decimal Value
    {
      get
      {
        return nudValue.Value;
      }
      set
      {
        if ((value > Maximum) || (value < Minimum))
        {
          ckbOverride.Checked = true;
        }
        else
        {
          ckbOverride.Checked = false;
          tkbSlider.Value = (Int32) (value*m_intDivisor);
        }
        nudValue.Value = value;
        RefreshEnabledStates();
      }
    }

    private void tkbSlider_Scroll(object sender, EventArgs e)
    {
      nudValue.Value = tkbSlider.Value/(decimal) m_intDivisor;
    }

    private void ckbOverride_CheckedChanged(object sender, EventArgs e)
    {
      RefreshEnabledStates();
    }

    protected void RefreshEnabledStates()
    {
      nudValue.Enabled = ckbOverride.Checked;
      tkbSlider.Enabled = !ckbOverride.Checked;
    }
  }
}